using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 게임 로그 매니저
/// 게임 플레이 데이터를 자동으로 수집하고 저장합니다
/// </summary>
public class GameLogManager : MonoBehaviour
{
    [Header("로그 설정")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private float autoSaveInterval = 30f; // 자동 저장 간격 (초)
    //[SerializeField] private int maxLogEntries = 1000; // 최대 로그 엔트리 수
    [SerializeField] private string logDirectory = "GameLogs";
    
    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;
    
    private GameLogData currentLogData;
    private string sessionId;
    private DateTime sessionStartTime;
    private Coroutine autoSaveCoroutine;
    private PlayerCharacter playerCharacter;
    private DungeonManager dungeonManager;
    
    // 싱글톤 패턴
    public static GameLogManager Instance { get; private set; }
    
    // 이벤트
    public System.Action<GameLogData> OnLogDataUpdated;
    public System.Action<string> OnLogSaved;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (enableLogging)
        {
            InitializeLogging();
        }
    }
    
    private void OnDestroy()
    {
        if (enableLogging)
        {
            SaveLogData();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (enableLogging && pauseStatus)
        {
            SaveLogData();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (enableLogging && !hasFocus)
        {
            SaveLogData();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (enableLogging)
        {
            // 세션 종료 정보 업데이트
            if (currentLogData != null && currentLogData.sessionData != null)
            {
                currentLogData.sessionData.endTime = DateTime.Now;
                currentLogData.sessionData.totalPlayTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;
                currentLogData.sessionData.completionReason = "ApplicationQuit";
            }
            
            SaveLogData();
            Debug.Log("게임 종료 시 로그가 저장되었습니다.");
        }
    }
    
    /// <summary>
    /// 로깅 시스템을 초기화합니다
    /// </summary>
    private void InitializeLogging()
    {
        // 세션 ID 생성
        sessionId = System.Guid.NewGuid().ToString();
        sessionStartTime = DateTime.Now;
        
        // 로그 데이터 초기화
        currentLogData = new GameLogData();
        currentLogData.sessionData = new GameSessionData
        {
            sessionId = sessionId,
            startTime = sessionStartTime,
            totalPlayTime = 0f,
            totalFloorsCleared = 0,
            isCompleted = false,
            completionReason = "InProgress"
        };
        
        // 컴포넌트 참조 찾기
        playerCharacter = FindFirstObjectByType<PlayerCharacter>();
        dungeonManager = FindFirstObjectByType<DungeonManager>();
        
        // 이벤트 구독
        SubscribeToEvents();
        
        // 자동 저장 시작
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
        autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        
        if (showDebugLogs)
        {
            Debug.Log($"게임 로깅 시작 - 세션 ID: {sessionId}");
        }
    }
    
    /// <summary>
    /// 이벤트들을 구독합니다
    /// </summary>
    private void SubscribeToEvents()
    {
        if (playerCharacter != null)
        {
            // 플레이어 이벤트 구독
            playerCharacter.OnHPChanged += OnPlayerHPChanged;
            playerCharacter.OnLevelUp += OnPlayerLevelUp;
            playerCharacter.OnExperienceChanged += OnPlayerExperienceChanged;
            playerCharacter.OnSkillPointsChanged += OnPlayerSkillPointsChanged;
        }
        
        if (dungeonManager != null)
        {
            // 던전 이벤트 구독
            dungeonManager.OnFloorChanged += OnFloorChanged;
            dungeonManager.OnFloorCleared += OnFloorCleared;
            dungeonManager.OnDungeonCompleted += OnDungeonCompleted;
            dungeonManager.OnDungeonFailed += OnDungeonFailed;
        }
    }
    
    /// <summary>
    /// 자동 저장 코루틴
    /// </summary>
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveLogData();
        }
    }
    
    /// <summary>
    /// 플레이어 HP 변경 로깅
    /// </summary>
    private void OnPlayerHPChanged(int currentHP, int maxHP)
    {
        if (currentLogData != null)
        {
            currentLogData.AddPlayerAction("HP_Change", "Self", playerCharacter.transform.position, currentHP, $"MaxHP: {maxHP}");
        }
    }
    
    /// <summary>
    /// 플레이어 레벨업 로깅
    /// </summary>
    private void OnPlayerLevelUp(int newLevel)
    {
        if (currentLogData != null)
        {
            currentLogData.AddPlayerAction("LevelUp", "Self", playerCharacter.transform.position, newLevel, $"New Level: {newLevel}");
            currentLogData.AddPlayerStats(playerCharacter);
        }
    }
    
    /// <summary>
    /// 플레이어 경험치 변경 로깅
    /// </summary>
    private void OnPlayerExperienceChanged(int currentExp, int expToNext)
    {
        if (currentLogData != null)
        {
            currentLogData.AddPlayerAction("ExperienceGain", "Self", playerCharacter.transform.position, currentExp, $"ToNext: {expToNext}");
        }
    }
    
    /// <summary>
    /// 플레이어 스킬 포인트 변경 로깅
    /// </summary>
    private void OnPlayerSkillPointsChanged(int skillPoints)
    {
        if (currentLogData != null)
        {
            currentLogData.AddPlayerAction("SkillPointsChange", "Self", playerCharacter.transform.position, skillPoints, $"Available: {skillPoints}");
        }
    }
    
    /// <summary>
    /// 던전 층 변경 로깅
    /// </summary>
    private void OnFloorChanged(int floorNumber)
    {
        if (currentLogData != null && dungeonManager != null)
        {
            var floorData = dungeonManager.GetType().GetField("currentFloorData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(dungeonManager);
            string floorName = floorData?.GetType().GetProperty("floorName")?.GetValue(floorData)?.ToString() ?? $"Floor {floorNumber}";
            
            currentLogData.AddDungeonProgress(floorNumber, floorName, 0, 0, 0f, "Started");
        }
    }
    
    /// <summary>
    /// 던전 층 클리어 로깅
    /// </summary>
    private void OnFloorCleared(DungeonFloorData floorData)
    {
        if (currentLogData != null)
        {
            currentLogData.sessionData.totalFloorsCleared++;
            currentLogData.AddDungeonProgress(currentLogData.sessionData.totalFloorsCleared, floorData.floorName, 0, 0, 0f, "Cleared");
        }
    }
    
    /// <summary>
    /// 던전 완료 로깅
    /// </summary>
    private void OnDungeonCompleted()
    {
        if (currentLogData != null)
        {
            currentLogData.sessionData.isCompleted = true;
            currentLogData.sessionData.completionReason = "Victory";
            currentLogData.sessionData.endTime = DateTime.Now;
            currentLogData.sessionData.totalPlayTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;
            
            SaveLogData();
        }
    }
    
    /// <summary>
    /// 던전 실패 로깅
    /// </summary>
    private void OnDungeonFailed()
    {
        if (currentLogData != null)
        {
            currentLogData.sessionData.isCompleted = true;
            currentLogData.sessionData.completionReason = "Defeat";
            currentLogData.sessionData.endTime = DateTime.Now;
            currentLogData.sessionData.totalPlayTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;
            
            SaveLogData();
        }
    }
    
    /// <summary>
    /// 전투 이벤트를 로깅합니다
    /// </summary>
    public void LogCombatEvent(string attackerName, string targetName, int damageDealt, bool isCritical, int attackerHP, int targetHP, string combatResult)
    {
        if (currentLogData != null && enableLogging)
        {
            currentLogData.AddCombatEvent(attackerName, targetName, damageDealt, isCritical, attackerHP, targetHP, combatResult);
            
            if (showDebugLogs)
            {
                Debug.Log($"전투 로그: {attackerName} -> {targetName}, 데미지: {damageDealt}, 크리티컬: {isCritical}");
            }
        }
    }
    
    /// <summary>
    /// 적 스폰을 로깅합니다
    /// </summary>
    public void LogEnemySpawn(string enemyType, Vector3 spawnPosition, int enemyLevel, bool isBoss)
    {
        if (currentLogData != null && enableLogging)
        {
            currentLogData.AddEnemySpawn(enemyType, spawnPosition, enemyLevel, isBoss);
            
            if (showDebugLogs)
            {
                Debug.Log($"적 스폰 로그: {enemyType} at {spawnPosition}, 레벨: {enemyLevel}, 보스: {isBoss}");
            }
        }
    }
    
    /// <summary>
    /// 플레이어 행동을 로깅합니다
    /// </summary>
    public void LogPlayerAction(string actionType, string targetName, Vector3 position, int value, string additionalInfo = "")
    {
        if (currentLogData != null && enableLogging)
        {
            currentLogData.AddPlayerAction(actionType, targetName, position, value, additionalInfo);
            
            if (showDebugLogs)
            {
                Debug.Log($"플레이어 행동 로그: {actionType} -> {targetName}, 값: {value}");
            }
        }
    }
    
    /// <summary>
    /// 플레이어 스탯을 로깅합니다
    /// </summary>
    public void LogPlayerStats()
    {
        if (currentLogData != null && playerCharacter != null && enableLogging)
        {
            currentLogData.AddPlayerStats(playerCharacter);
        }
    }
    
    /// <summary>
    /// 로그 데이터를 저장합니다
    /// </summary>
    public void SaveLogData()
    {
        if (currentLogData == null || !enableLogging) return;
        
        try
        {
            // 세션 종료 정보 업데이트 (아직 업데이트되지 않은 경우)
            if (currentLogData.sessionData != null && currentLogData.sessionData.endTime == DateTime.MinValue)
            {
                currentLogData.sessionData.endTime = DateTime.Now;
                currentLogData.sessionData.totalPlayTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;
            }
            
            // 로그 디렉토리 생성
            string logPath = Path.Combine(Application.persistentDataPath, logDirectory);
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            
            // 고정 파일명 사용 (덮어쓰기)
            string fileName = "GameLog.json";
            string filePath = Path.Combine(logPath, fileName);
            
            // JSON으로 직렬화
            string jsonData = JsonUtility.ToJson(currentLogData, true);
            
            // 파일 저장 (덮어쓰기)
            File.WriteAllText(filePath, jsonData);
            
            if (showDebugLogs)
            {
                Debug.Log($"게임 로그 저장 완료: {filePath}");
            }
            
            OnLogSaved?.Invoke(filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로그 저장 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 강제로 로그를 저장합니다 (종료 시 사용)
    /// </summary>
    public void ForceSaveLogData()
    {
        if (currentLogData == null) return;
        
        try
        {
            // 세션 종료 정보 강제 업데이트
            if (currentLogData.sessionData != null)
            {
                currentLogData.sessionData.endTime = DateTime.Now;
                currentLogData.sessionData.totalPlayTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;
                if (string.IsNullOrEmpty(currentLogData.sessionData.completionReason))
                {
                    currentLogData.sessionData.completionReason = "ForceSave";
                }
            }
            
            // 로그 디렉토리 생성
            string logPath = Path.Combine(Application.persistentDataPath, logDirectory);
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            
            // 고정 파일명 사용 (덮어쓰기)
            string fileName = "GameLog.json";
            string filePath = Path.Combine(logPath, fileName);
            
            // JSON으로 직렬화
            string jsonData = JsonUtility.ToJson(currentLogData, true);
            
            // 파일 저장 (덮어쓰기)
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"강제 로그 저장 완료: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"강제 로그 저장 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 로그 데이터를 가져옵니다
    /// </summary>
    public GameLogData GetLogData()
    {
        return currentLogData;
    }
    
    /// <summary>
    /// 로깅을 활성화/비활성화합니다
    /// </summary>
    public void SetLoggingEnabled(bool enabled)
    {
        enableLogging = enabled;
        if (showDebugLogs)
        {
            Debug.Log($"로깅 {(enabled ? "활성화" : "비활성화")}");
        }
    }
    
    /// <summary>
    /// 현재 세션 정보를 가져옵니다
    /// </summary>
    public GameSessionData GetSessionData()
    {
        return currentLogData?.sessionData;
    }
}
