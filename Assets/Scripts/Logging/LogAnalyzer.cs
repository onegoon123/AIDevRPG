using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 로그 데이터 분석기
/// 저장된 로그 파일들을 분석하여 게임 플레이 패턴을 파악합니다
/// </summary>
public class LogAnalyzer : MonoBehaviour
{
    [Header("분석 설정")]
    [SerializeField] private string logDirectory = "GameLogs";
    [SerializeField] private bool autoAnalyzeOnStart = true;
    
    [Header("분석 결과")]
    [SerializeField] private List<GameLogData> loadedLogs = new List<GameLogData>();
    [SerializeField] private AnalysisResult currentAnalysis;
    
    // 이벤트
    public System.Action<AnalysisResult> OnAnalysisCompleted;
    
    private void Start()
    {
        if (autoAnalyzeOnStart)
        {
            AnalyzeAllLogs();
        }
    }
    
    /// <summary>
    /// 최신 로그 파일을 분석합니다
    /// </summary>
    public void AnalyzeAllLogs()
    {
        try
        {
            string logPath = Path.Combine(Application.persistentDataPath, logDirectory);
            if (!Directory.Exists(logPath))
            {
                Debug.LogWarning("로그 디렉토리가 존재하지 않습니다.");
                return;
            }
            
            // 최신 로그 파일만 읽기
            string latestLogFile = Path.Combine(logPath, "GameLog.json");
            loadedLogs.Clear();
            
            if (File.Exists(latestLogFile))
            {
                try
                {
                    string jsonContent = File.ReadAllText(latestLogFile);
                    GameLogData logData = JsonUtility.FromJson<GameLogData>(jsonContent);
                    loadedLogs.Add(logData);
                    
                    currentAnalysis = PerformAnalysis(loadedLogs);
                    OnAnalysisCompleted?.Invoke(currentAnalysis);
                    
                    Debug.Log($"최신 로그 분석 완료: {latestLogFile}");
                    PrintAnalysisResults();
                }
                catch (Exception e)
                {
                    Debug.LogError($"로그 파일 로드 실패: {latestLogFile}, 오류: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("분석할 로그 파일이 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"로그 분석 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 로그 데이터를 분석합니다
    /// </summary>
    private AnalysisResult PerformAnalysis(List<GameLogData> logs)
    {
        AnalysisResult result = new AnalysisResult();
        
        // 기본 통계
        result.totalSessions = logs.Count;
        result.completedSessions = logs.Count(l => l.sessionData.isCompleted);
        result.averagePlayTime = (float)logs.Average(l => l.sessionData.totalPlayTime);
        result.averageFloorsCleared = (float)logs.Average(l => l.sessionData.totalFloorsCleared);
        
        // 플레이어 행동 분석
        AnalyzePlayerActions(logs, result);
        
        // 전투 분석
        AnalyzeCombatData(logs, result);
        
        // 던전 진행 분석
        AnalyzeDungeonProgress(logs, result);
        
        // 적 스폰 분석
        AnalyzeEnemySpawns(logs, result);
        
        return result;
    }
    
    /// <summary>
    /// 플레이어 행동을 분석합니다
    /// </summary>
    private void AnalyzePlayerActions(List<GameLogData> logs, AnalysisResult result)
    {
        var allActions = logs.SelectMany(l => l.playerActions).ToList();
        
        result.totalPlayerActions = allActions.Count;
        result.mostUsedSkill = allActions
            .Where(a => a.actionType == "SkillUpgrade")
            .GroupBy(a => a.targetName)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "None";
        
        result.totalExperienceGained = allActions
            .Where(a => a.actionType == "ExperienceGain")
            .Sum(a => a.value);
        
        result.totalHealingUsed = allActions
            .Where(a => a.actionType == "HealingSkill")
            .Count();
    }
    
    /// <summary>
    /// 전투 데이터를 분석합니다
    /// </summary>
    private void AnalyzeCombatData(List<GameLogData> logs, AnalysisResult result)
    {
        var allCombat = logs.SelectMany(l => l.combatEvents).ToList();
        
        result.totalCombatEvents = allCombat.Count;
        result.totalDamageDealt = allCombat.Sum(c => c.damageDealt);
        result.criticalHitRate = allCombat.Count > 0 ? 
            (float)allCombat.Count(c => c.isCritical) / allCombat.Count * 100f : 0f;
        
        result.averageDamagePerHit = allCombat.Count > 0 ? 
            (float)allCombat.Average(c => c.damageDealt) : 0f;
    }
    
    /// <summary>
    /// 던전 진행을 분석합니다
    /// </summary>
    private void AnalyzeDungeonProgress(List<GameLogData> logs, AnalysisResult result)
    {
        var allProgress = logs.SelectMany(l => l.dungeonProgress).ToList();
        
        result.totalFloorsAttempted = allProgress.Count;
        result.successfulFloors = allProgress.Count(p => p.floorResult == "Cleared");
        result.averageTimePerFloor = (float)allProgress.Average(p => p.timeSpentOnFloor);
        
        result.mostDifficultFloor = allProgress
            .Where(p => p.floorResult == "Failed")
            .GroupBy(p => p.floorNumber)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? 0;
    }
    
    /// <summary>
    /// 적 스폰을 분석합니다
    /// </summary>
    private void AnalyzeEnemySpawns(List<GameLogData> logs, AnalysisResult result)
    {
        var allSpawns = logs.SelectMany(l => l.enemySpawns).ToList();
        
        result.totalEnemiesSpawned = allSpawns.Count;
        result.bossEnemiesSpawned = allSpawns.Count(s => s.isBoss);
        
        result.mostCommonEnemyType = allSpawns
            .GroupBy(s => s.enemyType)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "None";
    }
    
    /// <summary>
    /// 분석 결과를 출력합니다
    /// </summary>
    private void PrintAnalysisResults()
    {
        if (currentAnalysis == null) return;
        
        Debug.Log("=== 게임 플레이 분석 결과 ===");
        Debug.Log($"총 세션 수: {currentAnalysis.totalSessions}");
        Debug.Log($"완료된 세션: {currentAnalysis.completedSessions}");
        Debug.Log($"평균 플레이 시간: {currentAnalysis.averagePlayTime:F1}초");
        Debug.Log($"평균 클리어 층수: {currentAnalysis.averageFloorsCleared:F1}");
        Debug.Log($"총 플레이어 행동: {currentAnalysis.totalPlayerActions}");
        Debug.Log($"가장 많이 사용된 스킬: {currentAnalysis.mostUsedSkill}");
        Debug.Log($"총 획득 경험치: {currentAnalysis.totalExperienceGained}");
        Debug.Log($"힐링 사용 횟수: {currentAnalysis.totalHealingUsed}");
        Debug.Log($"총 전투 이벤트: {currentAnalysis.totalCombatEvents}");
        Debug.Log($"총 입힌 데미지: {currentAnalysis.totalDamageDealt}");
        Debug.Log($"크리티컬 확률: {currentAnalysis.criticalHitRate:F1}%");
        Debug.Log($"평균 데미지: {currentAnalysis.averageDamagePerHit:F1}");
        Debug.Log($"총 시도한 층수: {currentAnalysis.totalFloorsAttempted}");
        Debug.Log($"성공한 층수: {currentAnalysis.successfulFloors}");
        Debug.Log($"평균 층당 시간: {currentAnalysis.averageTimePerFloor:F1}초");
        Debug.Log($"가장 어려운 층: {currentAnalysis.mostDifficultFloor}");
        Debug.Log($"총 스폰된 적: {currentAnalysis.totalEnemiesSpawned}");
        Debug.Log($"보스 적: {currentAnalysis.bossEnemiesSpawned}");
        Debug.Log($"가장 흔한 적 타입: {currentAnalysis.mostCommonEnemyType}");
    }
    
    /// <summary>
    /// 특정 세션의 상세 분석을 수행합니다
    /// </summary>
    public SessionAnalysisResult AnalyzeSession(string sessionId)
    {
        var session = loadedLogs.FirstOrDefault(l => l.sessionData.sessionId == sessionId);
        if (session == null)
        {
            Debug.LogWarning($"세션을 찾을 수 없습니다: {sessionId}");
            return null;
        }
        
        SessionAnalysisResult result = new SessionAnalysisResult
        {
            sessionId = sessionId,
            playTime = session.sessionData.totalPlayTime,
            floorsCleared = session.sessionData.totalFloorsCleared,
            isCompleted = session.sessionData.isCompleted,
            totalActions = session.playerActions.Count,
            totalCombatEvents = session.combatEvents.Count,
            totalEnemiesKilled = session.playerActions.Count(a => a.actionType == "EnemyKilled"),
            totalExperienceGained = session.playerActions.Where(a => a.actionType == "ExperienceGain").Sum(a => a.value),
            totalDamageDealt = session.combatEvents.Sum(c => c.damageDealt),
            criticalHits = session.combatEvents.Count(c => c.isCritical)
        };
        
        return result;
    }
    
    /// <summary>
    /// 로그 파일을 삭제합니다
    /// </summary>
    public void ClearAllLogs()
    {
        try
        {
            string logPath = Path.Combine(Application.persistentDataPath, logDirectory);
            if (Directory.Exists(logPath))
            {
                Directory.Delete(logPath, true);
                Debug.Log("모든 로그 파일이 삭제되었습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"로그 삭제 실패: {e.Message}");
        }
    }
}

/// <summary>
/// 분석 결과 데이터 구조
/// </summary>
[System.Serializable]
public class AnalysisResult
{
    public int totalSessions;
    public int completedSessions;
    public float averagePlayTime;
    public float averageFloorsCleared;
    
    public int totalPlayerActions;
    public string mostUsedSkill;
    public int totalExperienceGained;
    public int totalHealingUsed;
    
    public int totalCombatEvents;
    public int totalDamageDealt;
    public float criticalHitRate;
    public float averageDamagePerHit;
    
    public int totalFloorsAttempted;
    public int successfulFloors;
    public float averageTimePerFloor;
    public int mostDifficultFloor;
    
    public int totalEnemiesSpawned;
    public int bossEnemiesSpawned;
    public string mostCommonEnemyType;
}

/// <summary>
/// 세션별 분석 결과
/// </summary>
[System.Serializable]
public class SessionAnalysisResult
{
    public string sessionId;
    public float playTime;
    public int floorsCleared;
    public bool isCompleted;
    public int totalActions;
    public int totalCombatEvents;
    public int totalEnemiesKilled;
    public int totalExperienceGained;
    public int totalDamageDealt;
    public int criticalHits;
}
