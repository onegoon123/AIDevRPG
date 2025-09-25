using UnityEngine;
using System;

/// <summary>
/// 캐릭터의 기본 클래스
/// HP, 공격력, 방어력 등의 기본 속성을 포함
/// 2D 게임용 자동 추적 및 공격 시스템 포함
/// </summary>
public class Character : MonoBehaviour
{
    [Header("기본 속성")]
    [SerializeField] protected int maxHP = 100;
    [SerializeField] protected int currentHP;
    [SerializeField] protected int attackPower = 10;
    [SerializeField] protected int defense = 5;
    
    [Header("2D 전투 설정")]
    [SerializeField] protected float characterAttackRange = 1.5f;
    [SerializeField] protected float characterMoveSpeed = 3f;
    [SerializeField] protected float characterAttackCooldown = 1f;
    [SerializeField] protected LayerMask targetLayer = -1;
    
    [Header("상태")]
    [SerializeField] protected bool isAlive = true;
    [SerializeField] protected bool isAttacking = false;
    
    protected Character currentTarget;
    protected float characterLastAttackTime;
    
    // 이벤트
    public System.Action<int, int> OnHPChanged; // (현재 HP, 최대 HP)
    public System.Action OnCharacterDied;
    
    // 프로퍼티
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public int AttackPower => attackPower;
    public int Defense => defense;
    public bool IsAlive => isAlive;
    
    protected virtual void Start()
    {
        currentHP = maxHP;
        OnHPChanged?.Invoke(currentHP, maxHP);
    }
    
    protected virtual void Update()
    {
        if (!isAlive) return;
        
        // 자동으로 가장 가까운 적을 찾아서 추적 및 공격
        FindAndAttackNearestTarget();
    }
    
    /// <summary>
    /// 가장 가까운 적을 찾아서 추적 및 공격합니다
    /// </summary>
    protected virtual void FindAndAttackNearestTarget()
    {
        // 현재 타겟이 유효한지 확인
        if (currentTarget != null && !currentTarget.IsAlive)
        {
            currentTarget = null;
        }
        
        // 타겟이 없으면 새로운 타겟 찾기
        if (currentTarget == null)
        {
            currentTarget = FindNearestTarget();
        }
        
        // 타겟이 있으면 추적 및 공격
        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
            
            if (distanceToTarget <= characterAttackRange)
            {
                // 공격 범위 내에 있으면 공격
                if (Time.time - characterLastAttackTime >= characterAttackCooldown)
                {
                    Attack(currentTarget);
                    characterLastAttackTime = Time.time;
                }
            }
            else
            {
                // 공격 범위 밖에 있으면 추적
                MoveTowardsTarget();
            }
        }
    }
    
    /// <summary>
    /// 가장 가까운 적을 찾습니다
    /// </summary>
    /// <returns>가장 가까운 적 캐릭터</returns>
    protected virtual Character FindNearestTarget()
    {
        Character nearestTarget = null;
        float nearestDistance = float.MaxValue;
        
        // 모든 캐릭터를 검사
        Character[] allCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        
        foreach (Character character in allCharacters)
        {
            // 자신이 아니고, 살아있고, 적대적인 캐릭터인지 확인
            if (character != this && character.IsAlive && IsEnemy(character))
            {
                float distance = Vector2.Distance(transform.position, character.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = character;
                }
            }
        }
        
        return nearestTarget;
    }
    
    /// <summary>
    /// 타겟이 적인지 확인합니다 (오버라이드 가능)
    /// </summary>
    /// <param name="target">확인할 타겟</param>
    /// <returns>적이면 true</returns>
    protected virtual bool IsEnemy(Character target)
    {
        // 기본적으로 다른 타입의 캐릭터는 적으로 간주
        // PlayerCharacter는 EnemyCharacter를 적으로, EnemyCharacter는 PlayerCharacter를 적으로 간주
        return (this is PlayerCharacter && target is EnemyCharacter) ||
               (this is EnemyCharacter && target is PlayerCharacter);
    }
    
    /// <summary>
    /// 타겟을 향해 이동합니다
    /// </summary>
    protected virtual void MoveTowardsTarget()
    {
        if (currentTarget == null) return;
        
        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += (Vector3)direction * characterMoveSpeed * Time.deltaTime;
        
        // 이동 방향으로 회전 (2D에서는 Z축 회전)
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    /// <summary>
    /// HP를 설정합니다
    /// </summary>
    /// <param name="newHP">설정할 HP 값</param>
    public virtual void SetHP(int newHP)
    {
        currentHP = Mathf.Clamp(newHP, 0, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);
        
        if (currentHP <= 0 && isAlive)
        {
            Die();
        }
    }
    
    /// <summary>
    /// HP를 회복합니다
    /// </summary>
    /// <param name="amount">회복할 HP 양</param>
    public virtual void Heal(int amount)
    {
        if (!isAlive) return;
        
        SetHP(currentHP + amount);
    }
    
    /// <summary>
    /// 피해를 받습니다
    /// </summary>
    /// <param name="damage">받을 피해량</param>
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        // 방어력만큼 피해 감소 (최소 1의 피해는 받음)
        int actualDamage = Mathf.Max(1, damage - defense);
        SetHP(currentHP - actualDamage);
    }
    
    /// <summary>
    /// 다른 캐릭터를 공격합니다
    /// </summary>
    /// <param name="target">공격 대상</param>
    public virtual void Attack(Character target)
    {
        if (!isAlive || !target.IsAlive) return;
        
        // 공격 애니메이션 또는 이펙트를 위한 상태 설정
        isAttacking = true;
        
        // 피해 계산 및 적용
        int damage = CalculateDamage(target);
        target.TakeDamage(damage);
        
        // 전투 로깅 (PlayerCharacter가 아닌 경우에만)
        if (GameLogManager.Instance != null && !(this is PlayerCharacter))
        {
            GameLogManager.Instance.LogCombatEvent(
                gameObject.name, 
                target.name, 
                damage, 
                false, // 기본 Character는 크리티컬 없음
                currentHP, 
                target.CurrentHP, 
                "Hit"
            );
        }
        
        Debug.Log($"{gameObject.name}이(가) {target.gameObject.name}에게 {damage}의 피해를 입혔습니다.");
        
        // 공격 상태 해제 (애니메이션 완료 후)
        StartCoroutine(ResetAttackState());
    }
    
    /// <summary>
    /// 피해량을 계산합니다
    /// </summary>
    /// <param name="target">공격 대상</param>
    /// <returns>계산된 피해량</returns>
    protected virtual int CalculateDamage(Character target)
    {
        int baseDamage = attackPower;
        
        // 방어력만큼 피해 감소 (최소 1의 피해는 보장)
        int finalDamage = Mathf.Max(1, baseDamage - target.Defense);
        
        // 크리티컬 히트 확률 (10%)
        if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * 1.5f);
            Debug.Log("크리티컬 히트!");
        }
        
        return finalDamage;
    }
    
    /// <summary>
    /// 공격 상태를 리셋합니다
    /// </summary>
    private System.Collections.IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.5f); // 공격 애니메이션 시간
        isAttacking = false;
    }
    
    /// <summary>
    /// 캐릭터가 죽었을 때 호출됩니다
    /// </summary>
    protected virtual void Die()
    {
        isAlive = false;
        OnCharacterDied?.Invoke();
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 캐릭터를 부활시킵니다
    /// </summary>
    public virtual void Revive()
    {
        isAlive = true;
        SetHP(maxHP);
        Debug.Log($"{gameObject.name}이(가) 부활했습니다.");
    }
    
    /// <summary>
    /// 최대 HP를 설정합니다
    /// </summary>
    /// <param name="newMaxHP">새로운 최대 HP</param>
    public virtual void SetMaxHP(int newMaxHP)
    {
        maxHP = Mathf.Max(1, newMaxHP);
        if (currentHP > maxHP)
        {
            SetHP(maxHP);
        }
    }
    
    /// <summary>
    /// 공격력을 설정합니다
    /// </summary>
    /// <param name="newAttackPower">새로운 공격력</param>
    public virtual void SetAttackPower(int newAttackPower)
    {
        attackPower = Mathf.Max(0, newAttackPower);
    }
    
    /// <summary>
    /// 방어력을 설정합니다
    /// </summary>
    /// <param name="newDefense">새로운 방어력</param>
    public virtual void SetDefense(int newDefense)
    {
        defense = Mathf.Max(0, newDefense);
    }
}
