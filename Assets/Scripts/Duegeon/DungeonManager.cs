using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 던전 매니저 클래스
/// 던전의 전체적인 진행을 관리합니다
/// </summary>
public class DungeonManager : MonoBehaviour
{
    [Header("던전 설정")]
    [SerializeField] private DungeonData dungeonData;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnArea;
    
    [Header("적 스폰 설정")]
    [SerializeField] private float spawnRadius = 10f; // 스폰 반경
    [SerializeField] private float minDistanceFromPlayer = 3f; // 플레이어로부터 최소 거리
    [SerializeField] private int maxSpawnAttempts = 10; // 최대 스폰 시도 횟수
    
    [Header("UI 설정")]
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI dungeonStatusText;
    [SerializeField] private GameObject floorTransitionPanel;
    [SerializeField] private TextMeshProUGUI transitionText;
    
    [Header("던전 상태")]
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private bool isDungeonActive = false;
    [SerializeField] private bool isFloorCleared = false;
    
    private List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();
    private PlayerCharacter playerCharacter;
    private DungeonFloorData currentFloorData;
    private Coroutine spawnCoroutine;
    private bool isSpawningFloor = false;
    
    // 이벤트
    public System.Action<int> OnFloorChanged; // (새로운 층 번호)
    public System.Action OnDungeonCompleted;
    public System.Action OnDungeonFailed;
    public System.Action<DungeonFloorData> OnFloorCleared; // (클리어된 층 데이터)
    
    // 프로퍼티
    public int CurrentFloor => currentFloor;
    public bool IsDungeonActive => isDungeonActive;
    public bool IsFloorCleared => isFloorCleared;
    public int RemainingEnemies => spawnedEnemies.Count;
    
    private void Start()
    {
            InitializeDungeon();
    }
    
    private void Update()
    {
        if (isDungeonActive && !isFloorCleared)
        {
            CheckFloorClear();
            UpdateUI();
        }
    }
    
    /// <summary>
    /// 던전을 초기화합니다
    /// </summary>
    private void InitializeDungeon()
    {
        // 플레이어 캐릭터 찾기
        playerCharacter = FindFirstObjectByType<PlayerCharacter>();
        if (playerCharacter == null)
        {
            Debug.LogError("플레이어 캐릭터를 찾을 수 없습니다!");
            return;
        }
        
        // 던전 데이터 확인
        if (dungeonData == null)
        {
            Debug.LogError("던전 데이터가 설정되지 않았습니다!");
            return;
        }
        
        // 첫 번째 층 시작
        StartFloor(1);
    }
    
    /// <summary>
    /// 특정 층을 시작합니다
    /// </summary>
    /// <param name="floorNumber">시작할 층 번호</param>
    public void StartFloor(int floorNumber)
    {
        if (floorNumber < 1 || floorNumber > dungeonData.GetTotalFloors())
        {
            Debug.LogWarning($"유효하지 않은 층 번호입니다: {floorNumber}");
            return;
        }
        
        currentFloor = floorNumber;
        currentFloorData = dungeonData.GetFloorData(floorNumber);
        isFloorCleared = false;
        isDungeonActive = true;
        
        // 플레이어를 스폰 지점으로 이동
        if (playerSpawnPoint != null && playerCharacter != null)
        {
            playerCharacter.transform.position = playerSpawnPoint.position;
        }
        
        // 기존 적들 제거
        ClearSpawnedEnemies();
        
        // 새로운 적들 스폰
        StartCoroutine(SpawnEnemiesForFloor());
        
        // UI 업데이트
        UpdateUI();
        
        // 이벤트 발생
        OnFloorChanged?.Invoke(currentFloor);
        
        Debug.Log($"던전 {currentFloor}층 시작: {currentFloorData.floorName}");
    }
    
    /// <summary>
    /// 현재 층의 적들을 스폰합니다
    /// </summary>
    private IEnumerator SpawnEnemiesForFloor()
    {
        isSpawningFloor = true;
        if (currentFloorData.enemySpawns == null || currentFloorData.enemySpawns.Count == 0)
        {
            Debug.LogWarning("스폰할 적이 없습니다!");
            isSpawningFloor = false;
            yield break;
        }
        
        foreach (var enemySpawn in currentFloorData.enemySpawns)
        {
            for (int i = 0; i < enemySpawn.spawnCount; i++)
            {
                SpawnEnemy(enemySpawn);
                yield return new WaitForSeconds(enemySpawn.spawnDelay);
            }
        }
        
        isSpawningFloor = false;
        Debug.Log($"층 {currentFloor}의 모든 적이 스폰되었습니다. 총 {spawnedEnemies.Count}마리");
    }
    
    /// <summary>
    /// 적을 스폰합니다
    /// </summary>
    /// <param name="spawnData">스폰 데이터</param>
    private void SpawnEnemy(EnemySpawnData spawnData)
    {
        // 적 프리팹을 찾습니다 (실제 구현에서는 프리팹 매니저를 사용)
        GameObject enemyPrefab = GetEnemyPrefab(spawnData.enemyType);
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"적 프리팹을 찾을 수 없습니다: {spawnData.enemyType}");
            return;
        }
        
        // 랜덤 스폰 위치 계산
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        EnemyCharacter enemyCharacter = enemyObject.GetComponent<EnemyCharacter>();
        
        if (enemyCharacter != null)
        {
            // 적 타입 설정
            enemyCharacter.SetEnemyType(spawnData.enemyType);
            
            // 보스 설정
            if (spawnData.isBoss)
            {
                // 보스 특별 설정 (예: 크기, 색상 등)
                enemyObject.transform.localScale *= 1.5f;
            }
            
            // 스폰된 적 리스트에 추가
            spawnedEnemies.Add(enemyCharacter);
            
            // 적 사망 이벤트 구독
            enemyCharacter.OnEnemyDied += OnEnemyDied;
            
            // 적 스폰 로깅
            if (GameLogManager.Instance != null)
            {
                GameLogManager.Instance.LogEnemySpawn(
                    spawnData.enemyType.ToString(), 
                    spawnPosition, 
                    enemyCharacter.Level, 
                    spawnData.isBoss
                );
            }
        }
    }
    
    /// <summary>
    /// 랜덤한 스폰 위치를 계산합니다
    /// </summary>
    /// <returns>랜덤 스폰 위치</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 basePosition = enemySpawnArea != null ? enemySpawnArea.position : Vector3.zero;
        Vector3 playerPosition = playerCharacter != null ? playerCharacter.transform.position : Vector3.zero;
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // 랜덤 각도와 거리 계산
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDistance = Random.Range(0f, spawnRadius);
            
            // 랜덤 위치 계산
            Vector3 randomOffset = new Vector3(
                Mathf.Cos(randomAngle) * randomDistance,
                0f,
                Mathf.Sin(randomAngle) * randomDistance
            );
            
            Vector3 candidatePosition = basePosition + randomOffset;
            
            // 플레이어와의 거리 확인
            float distanceFromPlayer = Vector3.Distance(candidatePosition, playerPosition);
            if (distanceFromPlayer >= minDistanceFromPlayer)
            {
                return candidatePosition;
            }
        }
        
        // 최대 시도 횟수에 도달했을 경우 기본 위치 반환
        Debug.LogWarning("적절한 스폰 위치를 찾지 못했습니다. 기본 위치를 사용합니다.");
        return basePosition + new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
    }
    
    /// <summary>
    /// 적 타입에 따른 프리팹을 가져옵니다
    /// </summary>
    /// <param name="enemyType">적 타입</param>
    /// <returns>적 프리팹</returns>
    private GameObject GetEnemyPrefab(EnemyType enemyType)
    {
        // 실제 구현에서는 프리팹 매니저나 리소스 로더를 사용
        // 여기서는 기본 프리팹을 반환
        return Resources.Load<GameObject>($"Enemies/{enemyType}Enemy");
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
            
            // 플레이어에게 경험치와 골드 지급
            if (playerCharacter != null)
            {
                playerCharacter.GainExperience(enemy.ExperienceReward);
                // 골드 시스템이 있다면 여기서 골드 지급
            }
            
            // 적 처치 로깅
            if (GameLogManager.Instance != null)
            {
                GameLogManager.Instance.LogPlayerAction(
                    "EnemyKilled", 
                    enemy.Type.ToString(), 
                    enemy.transform.position, 
                    enemy.ExperienceReward, 
                    $"Remaining: {spawnedEnemies.Count}"
                );
            }
            
            Debug.Log($"적이 처치되었습니다. 남은 적: {spawnedEnemies.Count}");
        }
    }
    
    /// <summary>
    /// 층 클리어를 확인합니다
    /// </summary>
    private void CheckFloorClear()
    {
        if (!isSpawningFloor && spawnedEnemies.Count == 0 && !isFloorCleared)
        {
            ClearFloor();
        }
    }
    
    /// <summary>
    /// 층을 클리어합니다
    /// </summary>
    private void ClearFloor()
    {
        isFloorCleared = true;
        Debug.Log($"던전 {currentFloor}층 클리어!");
        
        // 층 클리어 로깅
        if (GameLogManager.Instance != null)
        {
            GameLogManager.Instance.LogPlayerAction(
                "FloorCleared", 
                $"Floor_{currentFloor}", 
                transform.position, 
                currentFloor, 
                $"Floor Name: {currentFloorData.floorName}"
            );
        }
        
        // 층 클리어 보상 지급
        GiveFloorRewards();
        
        // 이벤트 발생
        OnFloorCleared?.Invoke(currentFloorData);
        
        // 다음 층으로 이동 또는 던전 완료
        StartCoroutine(TransitionToNextFloor());
    }
    
    /// <summary>
    /// 층 클리어 보상을 지급합니다
    /// </summary>
    private void GiveFloorRewards()
    {
        if (playerCharacter != null)
        {
            // 경험치 보상
            int expReward = currentFloorData.experienceReward;
            if (expReward > 0)
            {
                playerCharacter.GainExperience(expReward);
            }
            
            // 골드 보상 (골드 시스템이 있다면)
            int goldReward = currentFloorData.goldReward;
            if (goldReward > 0)
            {
                // 골드 시스템 구현 필요
                Debug.Log($"골드 보상: {goldReward}");
            }
            
            // 아이템 보상
            foreach (var itemReward in currentFloorData.itemRewards)
            {
                if (Random.Range(0f, 1f) < itemReward.dropChance)
                {
                    // 아이템 시스템 구현 필요
                    Debug.Log($"아이템 보상: {itemReward.itemName} x{itemReward.itemCount}");
                }
            }
        }
    }
    
    /// <summary>
    /// 다음 층으로 전환합니다
    /// </summary>
    private IEnumerator TransitionToNextFloor()
    {
        // 전환 패널 표시
        if (floorTransitionPanel != null)
        {
            floorTransitionPanel.SetActive(true);
        }
        
        if (transitionText != null)
        {
            transitionText.text = $"{currentFloor}층 클리어!\n다음 층으로 이동합니다...";
        }
        
        yield return new WaitForSeconds(2f);
        
        // 다음 층으로 이동
        if (currentFloor < dungeonData.GetTotalFloors())
        {
            StartFloor(currentFloor + 1);
        }
        else
        {
            // 던전 완료
            CompleteDungeon();
        }
        
        // 전환 패널 숨기기
        if (floorTransitionPanel != null)
        {
            floorTransitionPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 던전을 완료합니다
    /// </summary>
    private void CompleteDungeon()
    {
        isDungeonActive = false;
        Debug.Log("던전 완료!");
        
        // 완료 보상 지급
        GiveCompletionRewards();
        
        // 이벤트 발생
        OnDungeonCompleted?.Invoke();
    }
    
    /// <summary>
    /// 던전 완료 보상을 지급합니다
    /// </summary>
    private void GiveCompletionRewards()
    {
        if (playerCharacter != null)
        {
            // 기본 완료 보상
            playerCharacter.GainExperience(dungeonData.baseExperienceReward);
            
            // 완료 아이템 보상
            foreach (var itemReward in dungeonData.completionRewards)
            {
                if (Random.Range(0f, 1f) < itemReward.dropChance)
                {
                    Debug.Log($"완료 보상: {itemReward.itemName} x{itemReward.itemCount}");
                }
            }
        }
    }
    
    /// <summary>
    /// 스폰된 적들을 모두 제거합니다
    /// </summary>
    private void ClearSpawnedEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        spawnedEnemies.Clear();
    }
    
    /// <summary>
    /// UI를 업데이트합니다
    /// </summary>
    private void UpdateUI()
    {
        if (floorText != null)
        {
            floorText.text = $"층: {currentFloor}";
        }
        
        if (enemyCountText != null)
        {
            enemyCountText.text = $"남은 적: {spawnedEnemies.Count}";
        }
        
        if (dungeonStatusText != null)
        {
            string status = isFloorCleared ? "층 클리어!" : "전투 중...";
            dungeonStatusText.text = status;
        }
    }
    
    /// <summary>
    /// 던전을 재시작합니다
    /// </summary>
    public void RestartDungeon()
    {
        ClearSpawnedEnemies();
        StartFloor(1);
    }
    
    /// <summary>
    /// 던전을 종료합니다
    /// </summary>
    public void ExitDungeon()
    {
        isDungeonActive = false;
        ClearSpawnedEnemies();
        
        // 던전 실패 이벤트 발생
        OnDungeonFailed?.Invoke();
    }
}
