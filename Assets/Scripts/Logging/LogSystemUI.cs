using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 로그 시스템 UI 매니저
/// 로그 시스템의 상태를 표시하고 제어할 수 있는 UI를 제공합니다
/// </summary>
public class LogSystemUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI logStatusText;
    [SerializeField] private TextMeshProUGUI sessionInfoText;
    [SerializeField] private TextMeshProUGUI analysisResultText;
    [SerializeField] private Button toggleLoggingButton;
    [SerializeField] private Button analyzeLogsButton;
    [SerializeField] private Button clearLogsButton;
    [SerializeField] private Button saveLogsButton;
    [SerializeField] private ScrollRect logScrollRect;
    [SerializeField] private TextMeshProUGUI logContentText;
    
    [Header("설정")]
    [SerializeField] private float updateInterval = 1f;
    
    private GameLogManager logManager;
    private LogAnalyzer logAnalyzer;
    private float lastUpdateTime;
    
    private void Start()
    {
        // 컴포넌트 참조 찾기
        logManager = GameLogManager.Instance;
        logAnalyzer = FindFirstObjectByType<LogAnalyzer>();
        
        // 버튼 이벤트 설정
        if (toggleLoggingButton != null)
            toggleLoggingButton.onClick.AddListener(ToggleLogging);
        
        if (analyzeLogsButton != null)
            analyzeLogsButton.onClick.AddListener(AnalyzeLogs);
        
        if (clearLogsButton != null)
            clearLogsButton.onClick.AddListener(ClearLogs);
        
        if (saveLogsButton != null)
            saveLogsButton.onClick.AddListener(SaveLogs);
        
        // 초기 UI 업데이트
        UpdateUI();
    }
    
    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// UI를 업데이트합니다
    /// </summary>
    private void UpdateUI()
    {
        UpdateLogStatus();
        UpdateSessionInfo();
        UpdateAnalysisResults();
    }
    
    /// <summary>
    /// 로그 상태를 업데이트합니다
    /// </summary>
    private void UpdateLogStatus()
    {
        if (logStatusText != null && logManager != null)
        {
            var sessionData = logManager.GetSessionData();
            if (sessionData != null)
            {
                logStatusText.text = $"로그 상태: 활성화\n" +
                                   $"세션 ID: {sessionData.sessionId}\n" +
                                   $"플레이 시간: {sessionData.totalPlayTime:F1}초\n" +
                                   $"클리어 층수: {sessionData.totalFloorsCleared}";
            }
            else
            {
                logStatusText.text = "로그 상태: 비활성화";
            }
        }
    }
    
    /// <summary>
    /// 세션 정보를 업데이트합니다
    /// </summary>
    private void UpdateSessionInfo()
    {
        if (sessionInfoText != null && logManager != null)
        {
            var logData = logManager.GetLogData();
            if (logData != null)
            {
                sessionInfoText.text = $"플레이어 행동: {logData.playerActions.Count}\n" +
                                     $"전투 이벤트: {logData.combatEvents.Count}\n" +
                                     $"던전 진행: {logData.dungeonProgress.Count}\n" +
                                     $"적 스폰: {logData.enemySpawns.Count}\n" +
                                     $"플레이어 스탯: {logData.playerStats.Count}";
            }
        }
    }
    
    /// <summary>
    /// 분석 결과를 업데이트합니다
    /// </summary>
    private void UpdateAnalysisResults()
    {
        if (analysisResultText != null && logAnalyzer != null)
        {
            // 분석 결과가 있으면 표시
            analysisResultText.text = "분석 결과를 보려면 '분석' 버튼을 클릭하세요.";
        }
    }
    
    /// <summary>
    /// 로깅을 토글합니다
    /// </summary>
    private void ToggleLogging()
    {
        if (logManager != null)
        {
            // 현재 상태를 확인하고 토글
            var sessionData = logManager.GetSessionData();
            bool isActive = sessionData != null;
            logManager.SetLoggingEnabled(!isActive);
            
            Debug.Log($"로깅 {(isActive ? "비활성화" : "활성화")}");
        }
    }
    
    /// <summary>
    /// 로그를 분석합니다
    /// </summary>
    private void AnalyzeLogs()
    {
        if (logAnalyzer != null)
        {
            logAnalyzer.AnalyzeAllLogs();
            
            if (analysisResultText != null)
            {
                analysisResultText.text = "로그 분석이 완료되었습니다. 콘솔을 확인하세요.";
            }
        }
    }
    
    /// <summary>
    /// 로그를 저장합니다
    /// </summary>
    private void SaveLogs()
    {
        if (logManager != null)
        {
            logManager.SaveLogData();
            Debug.Log("로그가 수동으로 저장되었습니다.");
        }
    }
    
    /// <summary>
    /// 모든 로그를 삭제합니다
    /// </summary>
    private void ClearLogs()
    {
        if (logAnalyzer != null)
        {
            logAnalyzer.ClearAllLogs();
            Debug.Log("모든 로그가 삭제되었습니다.");
        }
    }
    
    /// <summary>
    /// 로그 내용을 표시합니다
    /// </summary>
    public void ShowLogContent()
    {
        if (logContentText != null && logManager != null)
        {
            var logData = logManager.GetLogData();
            if (logData != null)
            {
                string content = "=== 현재 세션 로그 ===\n\n";
                
                // 플레이어 행동
                content += "플레이어 행동:\n";
                foreach (var action in logData.playerActions.TakeLast(10))
                {
                    string actionIcon = GetActionIcon(action.actionType);
                    content += $"- [{action.timestamp:HH:mm:ss.fff}] {actionIcon} {action.actionType}: {action.targetName}";
                    if (action.value > 0)
                    {
                        content += $" (값: {action.value})";
                    }
                    content += "\n";
                    if (!string.IsNullOrEmpty(action.additionalInfo))
                    {
                        content += $"  └ {action.additionalInfo}\n";
                    }
                }
                
                content += "\n전투 이벤트:\n";
                foreach (var combat in logData.combatEvents.TakeLast(10))
                {
                    content += $"- [{combat.timestamp:HH:mm:ss.fff}] {combat.attackerName} -> {combat.targetName}: {combat.damageDealt} 데미지";
                    if (combat.isCritical)
                    {
                        content += " (크리티컬!)";
                    }
                    content += $"\n  └ HP: {combat.attackerHP} -> {combat.targetHP}\n";
                }
                
                content += "\n던전 진행:\n";
                foreach (var progress in logData.dungeonProgress.TakeLast(5))
                {
                    content += $"- [{progress.timestamp:HH:mm:ss.fff}] 층 {progress.floorNumber}: {progress.floorResult}\n";
                    content += $"  └ 적 스폰: {progress.enemiesSpawned}, 처치: {progress.enemiesKilled}, 시간: {progress.timeSpentOnFloor:F1}초\n";
                }
                
                logContentText.text = content;
            }
        }
    }
    
    /// <summary>
    /// 행동 타입에 따른 아이콘을 반환합니다
    /// </summary>
    private string GetActionIcon(string actionType)
    {
        switch (actionType)
        {
            case "ExperienceGain": return "⭐";
            case "LevelUp": return "⬆️";
            case "SkillUpgrade": return "🔧";
            case "HealingSkill": return "💚";
            case "EnemyKilled": return "⚔️";
            case "CharacterDied": return "💀";
            case "EnemyDied": return "💀";
            case "FloorCleared": return "🏆";
            case "HP_Change": return "❤️";
            default: return "📝";
        }
    }
}
