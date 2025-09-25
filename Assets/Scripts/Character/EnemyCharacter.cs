using UnityEngine;
using System.Collections;

/// <summary>
/// 적 캐릭터 클래스
/// Character 클래스를 상속받아 적 특화 기능을 추가
/// </summary>
public class EnemyCharacter : Character
{
    [Header("적 특화 속성")]
    [SerializeField] private EnemyType enemyType = EnemyType.Normal;
    [SerializeField] private int experienceReward = 10;
    [SerializeField] private int goldReward = 5;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float enemyAttackRange = 2f;
    [SerializeField] private float enemyMoveSpeed = 2f;
    
    [Header("AI 설정")]
    [SerializeField] private float enemyAttackCooldown = 2f;
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private Transform patrolCenter;
    
    private Transform player;
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private EnemyState currentState = EnemyState.Patrol;
    private float enemyLastAttackTime;
    private float lastStateChangeTime;
    
    // 이벤트
    public System.Action<EnemyCharacter> OnEnemyDied;
    public System.Action<int, int> OnRewardGiven; // (경험치, 골드)
    
    // 프로퍼티
    public EnemyType Type => enemyType;
    public int ExperienceReward => experienceReward;
    public int GoldReward => goldReward;
    public EnemyState CurrentState => currentState;
    
    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        patrolCenter = patrolCenter != null ? patrolCenter : transform;
        SetNewPatrolTarget();
        
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    protected override void Update()
    {
        if (!isAlive) return;
        
        // 기본 Character의 자동 전투 시스템을 사용하되, AI 상태에 따라 동작 제어
        if (currentState == EnemyState.Patrol)
        {
            // 순찰 중일 때는 자동 전투 비활성화
            Patrol();
        }
        else
        {
            // 추적/공격 상태일 때는 기본 자동 전투 시스템 사용
            base.Update();
        }
    }
    
    /// <summary>
    /// 순찰 상태
    /// </summary>
    private void Patrol()
    {
        // 플레이어 감지
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // 순찰 지점으로 이동
        if (Vector2.Distance(transform.position, patrolTarget) > 0.5f)
        {
            MoveTowards(patrolTarget);
        }
        else
        {
            // 새로운 순찰 지점 설정
            SetNewPatrolTarget();
        }
    }
    
    /// <summary>
    /// 추적 상태
    /// </summary>
    private void Chase()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 공격 범위 내에 있으면 공격 상태로 전환
        if (distanceToPlayer <= enemyAttackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        
        // 플레이어가 감지 범위를 벗어나면 복귀
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            ChangeState(EnemyState.Return);
            return;
        }
        
        // 플레이어를 향해 이동
        MoveTowards(player.position);
    }
    
    /// <summary>
    /// 공격 상태
    /// </summary>
    private void AttackPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 공격 범위를 벗어나면 추적 상태로 전환
        if (distanceToPlayer > enemyAttackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // 공격 쿨다운 확인
        if (Time.time - enemyLastAttackTime >= enemyAttackCooldown)
        {
            PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                Attack(playerCharacter);
                enemyLastAttackTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 복귀 상태
    /// </summary>
    private void ReturnToStart()
    {
        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        
        if (distanceToStart > 0.5f)
        {
            MoveTowards(startPosition);
        }
        else
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    /// <summary>
    /// 지정된 위치로 이동
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    private void MoveTowards(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        transform.position += (Vector3)direction * enemyMoveSpeed * Time.deltaTime;
        
        // 이동 방향으로 회전 (2D에서는 Z축 회전)
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    /// <summary>
    /// 새로운 순찰 지점을 설정합니다
    /// </summary>
    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection.y = 0; // Y축은 고정
        patrolTarget = patrolCenter.position + randomDirection;
    }
    
    /// <summary>
    /// 상태를 변경합니다
    /// </summary>
    /// <param name="newState">새로운 상태</param>
    private void ChangeState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            lastStateChangeTime = Time.time;
            Debug.Log($"{gameObject.name}의 상태가 {newState}로 변경되었습니다.");
        }
    }
    
    /// <summary>
    /// 적이 죽었을 때 호출됩니다
    /// </summary>
    protected override void Die()
    {
        base.Die();
        OnEnemyDied?.Invoke(this);
        
        // 플레이어에게 보상 지급
        if (player != null)
        {
            PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.GainExperience(experienceReward);
                OnRewardGiven?.Invoke(experienceReward, goldReward);
                Debug.Log($"플레이어가 {experienceReward} 경험치와 {goldReward} 골드를 획득했습니다.");
            }
        }
    }
    
    /// <summary>
    /// 적 타입에 따른 능력치 설정
    /// </summary>
    /// <param name="type">적 타입</param>
    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;
        
        switch (type)
        {
            case EnemyType.Weak:
                SetMaxHP(50);
                SetAttackPower(5);
                SetDefense(1);
                experienceReward = 5;
                goldReward = 2;
                break;
            case EnemyType.Normal:
                SetMaxHP(100);
                SetAttackPower(10);
                SetDefense(3);
                experienceReward = 10;
                goldReward = 5;
                break;
            case EnemyType.Strong:
                SetMaxHP(200);
                SetAttackPower(20);
                SetDefense(8);
                experienceReward = 25;
                goldReward = 15;
                break;
            case EnemyType.Boss:
                SetMaxHP(500);
                SetAttackPower(35);
                SetDefense(15);
                experienceReward = 100;
                goldReward = 50;
                break;
        }
    }
    
    /// <summary>
    /// 감지 범위를 설정합니다
    /// </summary>
    /// <param name="range">새로운 감지 범위</param>
    public void SetDetectionRange(float range)
    {
        detectionRange = Mathf.Max(0f, range);
    }
    
    /// <summary>
    /// 이동 속도를 설정합니다
    /// </summary>
    /// <param name="speed">새로운 이동 속도</param>
    public void SetMoveSpeed(float speed)
    {
        enemyMoveSpeed = Mathf.Max(0f, speed);
    }
}

/// <summary>
/// 적 타입 열거형
/// </summary>
public enum EnemyType
{
    Weak,   // 약한 적
    Normal, // 일반 적
    Strong, // 강한 적
    Boss    // 보스
}

/// <summary>
/// 적 상태 열거형
/// </summary>
public enum EnemyState
{
    Patrol, // 순찰
    Chase,  // 추적
    Attack, // 공격
    Return  // 복귀
}
