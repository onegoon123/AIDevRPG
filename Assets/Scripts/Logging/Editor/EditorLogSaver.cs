using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// 에디터에서 게임 종료 시 로그를 저장하는 클래스
/// 에디터 전용 기능으로 게임 종료 시 자동으로 로그를 저장합니다
/// </summary>
[InitializeOnLoad]
public static class EditorLogSaver
{
    private static bool isGamePlaying = false;
    private static DateTime gameStartTime;
    
    static EditorLogSaver()
    {
        // 에디터 이벤트 구독
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += OnEditorQuitting;
        
        // 게임 시작/중지 감지
        EditorApplication.pauseStateChanged += OnPauseStateChanged;
    }
    
    /// <summary>
    /// 플레이 모드 상태 변경 시 호출
    /// </summary>
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                isGamePlaying = true;
                gameStartTime = DateTime.Now;
                Debug.Log("게임 시작 - 로그 수집 시작");
                break;
                
            case PlayModeStateChange.ExitingPlayMode:
                if (isGamePlaying)
                {
                    SaveLogOnGameStop();
                    isGamePlaying = false;
                }
                break;
        }
    }
    
    /// <summary>
    /// 일시정지 상태 변경 시 호출
    /// </summary>
    private static void OnPauseStateChanged(PauseState pauseState)
    {
        if (isGamePlaying)
        {
            switch (pauseState)
            {
                case PauseState.Paused:
                    Debug.Log("게임 일시정지 - 로그 저장");
                    SaveLogData();
                    break;
                    
                case PauseState.Unpaused:
                    Debug.Log("게임 재개");
                    break;
            }
        }
    }
    
    /// <summary>
    /// 에디터 종료 시 호출
    /// </summary>
    private static void OnEditorQuitting()
    {
        if (isGamePlaying)
        {
            Debug.Log("에디터 종료 - 로그 저장");
            SaveLogData();
        }
    }
    
    /// <summary>
    /// 게임 중지 시 로그 저장
    /// </summary>
    private static void SaveLogOnGameStop()
    {
        try
        {
            // GameLogManager 인스턴스 찾기
            var logManager = GameLogManager.Instance;
            if (logManager != null)
            {
                // 세션 종료 정보 업데이트
                var sessionData = logManager.GetSessionData();
                if (sessionData != null)
                {
                    sessionData.endTime = DateTime.Now;
                    sessionData.totalPlayTime = (float)(DateTime.Now - gameStartTime).TotalSeconds;
                    sessionData.completionReason = "EditorStop";
                }
                
                // 로그 저장
                logManager.SaveLogData();
                Debug.Log("게임 중지 시 로그가 저장되었습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 중지 시 로그 저장 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 로그 데이터 저장
    /// </summary>
    private static void SaveLogData()
    {
        try
        {
            var logManager = GameLogManager.Instance;
            if (logManager != null)
            {
                logManager.SaveLogData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"로그 저장 실패: {e.Message}");
        }
    }
}
