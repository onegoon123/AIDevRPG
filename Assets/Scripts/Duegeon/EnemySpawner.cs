using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 적 스폰 시스템
/// 던전에서 적을 스폰하고 관리합니다
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private Transform spawnArea;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private bool useRandomSpawn = true;
    [SerializeField] private LayerMask spawnLayerMask = -1;
    
    [Header("적 프리팹")]
    [SerializeField] private GameObject weakEnemyPrefab;
    [SerializeField] private GameObject normalEnemyPrefab;
    [SerializeField] private GameObject strongEnemyPrefab;
    [SerializeField] private GameObject bossEnemyPrefab;
    
    [Header("스폰 제한")]
    [SerializeField] private int maxEnemiesPerSpawn = 10;
    [SerializeField] private float spawnCooldown = 1f;
    [SerializeField] private bool limitTotalEnemies = true;
    [SerializeField] private int maxTotalEnemies = 50;
    
    private List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();
    private Dictionary<EnemyType, GameObject> enemyPrefabs = new Dictionary<EnemyType, GameObject>();
    private bool isSpawning = false;
    private int remainingToSpawn = 0;
    
    // 이벤트
    public System.Action<EnemyCharacter> OnEnemySpawned;
    public System.Action<EnemyCharacter> OnEnemyDestroyed;
    public System.Action<int> OnEnemyCountChanged; // (현재 적 수)
    public System.Action OnSpawningCompleted; // 모든 예정 스폰 완료 시
    
    // 프로퍼티
    public int SpawnedEnemyCount => spawnedEnemies.Count;
    public bool IsSpawning => isSpawning;
    public int RemainingToSpawn => remainingToSpawn;
    public bool HasPendingSpawns => isSpawning || remainingToSpawn > 0;
    
    private void Start()
    {
            InitializeEnemyPrefabs();
    }
    
    /// <summary>
    /// 적 프리팹을 초기화합니다
    /// </summary>
    private void InitializeEnemyPrefabs()
    {
        enemyPrefabs.Clear();
        
        // 프리팹 매핑
        if (weakEnemyPrefab != null)
            enemyPrefabs[EnemyType.Weak] = weakEnemyPrefab;
        
        if (normalEnemyPrefab != null)
            enemyPrefabs[EnemyType.Normal] = normalEnemyPrefab;
        
        if (strongEnemyPrefab != null)
            enemyPrefabs[EnemyType.Strong] = strongEnemyPrefab;
        
        if (bossEnemyPrefab != null)
            enemyPrefabs[EnemyType.Boss] = bossEnemyPrefab;
        
        Debug.Log($"적 프리팹 초기화 완료: {enemyPrefabs.Count}개");
    }
    
    /// <summary>
    /// 적을 스폰합니다
    /// </summary>
    /// <param name="enemyType">적 타입</param>
    /// <param name="spawnPosition">스폰 위치</param>
    /// <param name="isBoss">보스 여부</param>
    /// <returns>스폰된 적</returns>
    public EnemyCharacter SpawnEnemy(EnemyType enemyType, Vector2 spawnPosition, bool isBoss = false)
    {
        // 최대 적 수 제한 확인
        if (limitTotalEnemies && spawnedEnemies.Count >= maxTotalEnemies)
        {
            Debug.LogWarning("최대 적 수에 도달했습니다!");
            return null;
        }
        
        // 프리팹 확인
        if (!enemyPrefabs.ContainsKey(enemyType))
        {
            Debug.LogWarning($"적 프리팹을 찾을 수 없습니다: {enemyType}");
            return null;
        }
        
        // 스폰 위치 계산
        Vector3 finalSpawnPosition = CalculateSpawnPosition(spawnPosition);
        
        // 적 스폰
        GameObject enemyObject = Instantiate(enemyPrefabs[enemyType], finalSpawnPosition, Quaternion.identity);
        EnemyCharacter enemyCharacter = enemyObject.GetComponent<EnemyCharacter>();
        
        if (enemyCharacter != null)
        {
            // 적 설정
            enemyCharacter.SetEnemyType(enemyType);
            
            // 보스 설정
            if (isBoss)
            {
                SetupBossEnemy(enemyCharacter);
            }
            
            // 스폰된 적 리스트에 추가
            spawnedEnemies.Add(enemyCharacter);
            
            // 이벤트 구독
            enemyCharacter.OnEnemyDied += OnEnemyDied;
            
            // 이벤트 발생
            OnEnemySpawned?.Invoke(enemyCharacter);
            OnEnemyCountChanged?.Invoke(spawnedEnemies.Count);
            
            Debug.Log($"적 스폰 완료: {enemyType} at {finalSpawnPosition}");
        }
        
        return enemyCharacter;
    }
    
    /// <summary>
    /// 여러 적을 스폰합니다
    /// </summary>
    /// <param name="spawnData">스폰 데이터</param>
    public void SpawnEnemies(List<EnemySpawnData> spawnData)
    {
        if (isSpawning) return;
        
        // 남은 스폰 수 계산
        remainingToSpawn = 0;
        if (spawnData != null)
        {
            for (int i = 0; i < spawnData.Count; i++)
            {
                if (spawnData[i].spawnCount > 0) remainingToSpawn += spawnData[i].spawnCount;
            }
        }
        
        StartCoroutine(SpawnEnemiesCoroutine(spawnData));
    }
    
    /// <summary>
    /// 적 스폰 코루틴
    /// </summary>
    private IEnumerator SpawnEnemiesCoroutine(List<EnemySpawnData> spawnData)
    {
        isSpawning = true;
        
        foreach (var data in spawnData)
        {
            for (int i = 0; i < data.spawnCount; i++)
            {
                // 스폰 제한 확인
                if (limitTotalEnemies && spawnedEnemies.Count >= maxTotalEnemies)
                {
                    Debug.LogWarning("최대 적 수에 도달하여 스폰을 중단합니다.");
                    break;
                }
                
                // 스폰당 최대 적 수 제한 확인
                if (spawnedEnemies.Count >= maxEnemiesPerSpawn)
                {
                    Debug.LogWarning($"스폰당 최대 적 수({maxEnemiesPerSpawn})에 도달했습니다.");
                    break;
                }
                
                // 적 스폰
                SpawnEnemy(data.enemyType, data.spawnPosition, data.isBoss);
                if (remainingToSpawn > 0) remainingToSpawn--;
                
                // 스폰 쿨다운
                if (spawnCooldown > 0f)
                {
                    yield return new WaitForSeconds(spawnCooldown);
                }
            }
        }
        
        isSpawning = false;
        remainingToSpawn = 0;
        OnSpawningCompleted?.Invoke();
        Debug.Log($"적 스폰 완료: 총 {spawnedEnemies.Count}마리");
    }
    
    /// <summary>
    /// 스폰 위치를 계산합니다
    /// </summary>
    /// <param name="requestedPosition">요청된 위치</param>
    /// <returns>최종 스폰 위치</returns>
    private Vector3 CalculateSpawnPosition(Vector2 requestedPosition)
    {
        Vector3 basePosition = spawnArea != null ? spawnArea.position : transform.position;
        
        if (useRandomSpawn)
        {
            // 랜덤 위치 생성
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return basePosition + (Vector3)(requestedPosition + randomOffset);
        }
        else
        {
            // 지정된 위치 사용
            return basePosition + (Vector3)requestedPosition;
        }
    }
    
    /// <summary>
    /// 보스 적을 설정합니다
    /// </summary>
    /// <param name="enemy">보스 적</param>
    private void SetupBossEnemy(EnemyCharacter enemy)
    {
        // 보스 특별 설정
        enemy.transform.localScale *= 1.5f; // 크기 증가
        
        // 보스 색상 변경 (예시)
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        // 보스 특별 능력치 (예시)
        enemy.SetMaxHP(enemy.MaxHP * 2);
        enemy.SetAttackPower(enemy.AttackPower * 2);
        
        Debug.Log("보스 적 설정 완료");
    }
    
    /// <summary>
    /// 적이 죽었을 때 호출됩니다
    /// </summary>
    /// <param name="enemy">죽은 적</param>
    private void OnEnemyDied(EnemyCharacter enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
            
            // 이벤트 발생
            OnEnemyDestroyed?.Invoke(enemy);
            OnEnemyCountChanged?.Invoke(spawnedEnemies.Count);
            
            Debug.Log($"적이 제거되었습니다. 남은 적: {spawnedEnemies.Count}");
        }
    }
    
    /// <summary>
    /// 모든 적을 제거합니다
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        
        spawnedEnemies.Clear();
        OnEnemyCountChanged?.Invoke(0);
        
        Debug.Log("모든 적이 제거되었습니다.");
    }
    
    /// <summary>
    /// 특정 타입의 적 수를 반환합니다
    /// </summary>
    /// <param name="enemyType">적 타입</param>
    /// <returns>해당 타입의 적 수</returns>
    public int GetEnemyCountByType(EnemyType enemyType)
    {
        int count = 0;
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.Type == enemyType)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// 살아있는 적이 있는지 확인합니다
    /// </summary>
    /// <returns>살아있는 적이 있으면 true</returns>
    public bool HasAliveEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 현재 웨이브(스폰 포함)가 완료되었는지 여부
    /// </summary>
    public bool IsWaveComplete()
    {
        return !HasPendingSpawns && !HasAliveEnemies();
    }
    
    /// <summary>
    /// 스폰 영역을 설정합니다
    /// </summary>
    /// <param name="area">스폰 영역</param>
    public void SetSpawnArea(Transform area)
    {
        spawnArea = area;
    }
    
    /// <summary>
    /// 스폰 반경을 설정합니다
    /// </summary>
    /// <param name="radius">새로운 반경</param>
    public void SetSpawnRadius(float radius)
    {
        spawnRadius = Mathf.Max(0f, radius);
    }
    
    /// <summary>
    /// 최대 적 수를 설정합니다
    /// </summary>
    /// <param name="maxCount">최대 적 수</param>
    public void SetMaxEnemies(int maxCount)
    {
        maxTotalEnemies = Mathf.Max(1, maxCount);
    }
    
    /// <summary>
    /// 스폰 쿨다운을 설정합니다
    /// </summary>
    /// <param name="cooldown">새로운 쿨다운</param>
    public void SetSpawnCooldown(float cooldown)
    {
        spawnCooldown = Mathf.Max(0f, cooldown);
    }
    
    /// <summary>
    /// 적 프리팹을 설정합니다
    /// </summary>
    /// <param name="enemyType">적 타입</param>
    /// <param name="prefab">프리팹</param>
    public void SetEnemyPrefab(EnemyType enemyType, GameObject prefab)
    {
        if (prefab != null)
        {
            enemyPrefabs[enemyType] = prefab;
        }
    }
    
    /// <summary>
    /// 스폰 중인지 확인합니다
    /// </summary>
    public bool IsCurrentlySpawning => isSpawning;
    
    /// <summary>
    /// 스폰된 적 리스트를 반환합니다
    /// </summary>
    public List<EnemyCharacter> GetSpawnedEnemies()
    {
        return new List<EnemyCharacter>(spawnedEnemies);
    }
}
