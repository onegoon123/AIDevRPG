using UnityEngine;

/// <summary>
/// 플레이어 캐릭터 클래스
/// Character 클래스를 상속받아 플레이어 특화 기능을 추가
/// </summary>
public class PlayerCharacter : Character
{
    [Header("플레이어 특화 속성")]
    [SerializeField] private int experience = 0;
    [SerializeField] private int level = 1;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private int skillPoints = 0;
    
    [Header("플레이어 스킬")]
    [SerializeField] private int healingSkill = 0;
    [SerializeField] private int criticalChance = 0;
    [SerializeField] private int damageBoost = 0;
    
    // 이벤트
    public System.Action<int, int> OnExperienceChanged; // (현재 경험치, 다음 레벨까지)
    public System.Action<int> OnLevelUp; // (새로운 레벨)
    public System.Action<int> OnSkillPointsChanged; // (스킬 포인트)
    
    // 프로퍼티
    public int Experience => experience;
    public int Level => level;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int SkillPoints => skillPoints;
    
    protected override void Start()
    {
        base.Start();
        OnExperienceChanged?.Invoke(experience, experienceToNextLevel);
        OnLevelUp?.Invoke(level);
        OnSkillPointsChanged?.Invoke(skillPoints);
    }
    
    /// <summary>
    /// 경험치를 획득합니다
    /// </summary>
    /// <param name="amount">획득할 경험치 양</param>
    public void GainExperience(int amount)
    {
        experience += amount;
        OnExperienceChanged?.Invoke(experience, experienceToNextLevel);
        
        // 레벨업 체크
        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }
    
    /// <summary>
    /// 레벨업을 수행합니다
    /// </summary>
    private void LevelUp()
    {
        experience -= experienceToNextLevel;
        level++;
        skillPoints += 2; // 레벨업 시 스킬 포인트 2개 획득
        
        // 레벨업 시 능력치 증가
        SetMaxHP(maxHP + 20);
        SetAttackPower(attackPower + 5);
        SetDefense(defense + 2);
        
        // HP 회복
        Heal(50);
        
        // 다음 레벨까지 필요한 경험치 증가
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
        
        OnLevelUp?.Invoke(level);
        OnSkillPointsChanged?.Invoke(skillPoints);
        OnExperienceChanged?.Invoke(experience, experienceToNextLevel);
        
        Debug.Log($"레벨업! 현재 레벨: {level}");
    }
    
    /// <summary>
    /// 스킬을 업그레이드합니다
    /// </summary>
    /// <param name="skillType">업그레이드할 스킬 타입</param>
    public bool UpgradeSkill(SkillType skillType)
    {
        if (skillPoints <= 0) return false;
        
        switch (skillType)
        {
            case SkillType.Healing:
                healingSkill++;
                break;
            case SkillType.CriticalChance:
                criticalChance++;
                break;
            case SkillType.DamageBoost:
                damageBoost++;
                break;
        }
        
        skillPoints--;
        OnSkillPointsChanged?.Invoke(skillPoints);
        
        Debug.Log($"{skillType} 스킬이 업그레이드되었습니다.");
        return true;
    }
    
    /// <summary>
    /// 힐링 스킬을 사용합니다
    /// </summary>
    public void UseHealingSkill()
    {
        if (healingSkill <= 0) return;
        
        int healAmount = 20 + (healingSkill * 10);
        Heal(healAmount);
        
        Debug.Log($"힐링 스킬을 사용하여 {healAmount}만큼 회복했습니다.");
    }
    
    /// <summary>
    /// 다른 캐릭터를 공격합니다 (크리티컬 확률 적용)
    /// </summary>
    /// <param name="target">공격 대상</param>
    public override void Attack(Character target)
    {
        if (!isAlive || !target.IsAlive) return;
        
        // 공격 애니메이션 또는 이펙트를 위한 상태 설정
        isAttacking = true;
        
        int baseDamage = attackPower;
        
        // 데미지 부스트 적용
        if (damageBoost > 0)
        {
            baseDamage += damageBoost * 2;
        }
        
        // 크리티컬 확률 적용
        float critChance = 0.1f + (criticalChance * 0.05f); // 기본 10% + 스킬당 5%
        if (Random.Range(0f, 1f) < critChance)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * 2f);
            Debug.Log("크리티컬 히트!");
        }
        
        // 방어력만큼 피해 감소 (최소 1의 피해는 보장)
        int finalDamage = Mathf.Max(1, baseDamage - target.Defense);
        target.TakeDamage(finalDamage);
        
        Debug.Log($"{gameObject.name}이(가) {target.name}에게 {finalDamage}의 피해를 입혔습니다.");
        
        // 공격 상태 해제 (애니메이션 완료 후)
        StartCoroutine(ResetAttackState());
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
    /// 스킬 레벨을 반환합니다
    /// </summary>
    /// <param name="skillType">확인할 스킬 타입</param>
    /// <returns>스킬 레벨</returns>
    public int GetSkillLevel(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Healing:
                return healingSkill;
            case SkillType.CriticalChance:
                return criticalChance;
            case SkillType.DamageBoost:
                return damageBoost;
            default:
                return 0;
        }
    }
}

/// <summary>
/// 스킬 타입 열거형
/// </summary>
public enum SkillType
{
    Healing,        // 힐링 스킬
    CriticalChance, // 크리티컬 확률
    DamageBoost     // 데미지 부스트
}
