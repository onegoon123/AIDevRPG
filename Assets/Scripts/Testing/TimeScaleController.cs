using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 시간 배속을 제어하는 클래스
/// 테스팅과 디버깅을 위한 시간 조절 기능을 제공합니다
/// </summary>
public class TimeScaleController : MonoBehaviour
{
    [Header("배속 설정")]
    [SerializeField] private float[] presetSpeeds = { 0.25f, 0.5f, 1f, 2f, 4f, 8f, 16f };
    [SerializeField] private int currentSpeedIndex = 2; // 기본값: 1x (인덱스 2)
    [SerializeField] private float customSpeed = 1f;
    [SerializeField] private bool useCustomSpeed = false;
    
    [Header("UI 설정")]
    [SerializeField] private bool showUI = true;
    [SerializeField] private KeyCode toggleUIKey = KeyCode.F1;
    [SerializeField] private KeyCode pauseKey = KeyCode.Space;
    [SerializeField] private KeyCode resetSpeedKey = KeyCode.R;
    
    [Header("키보드 단축키")]
    [SerializeField] private KeyCode speedUpKey = KeyCode.Equals; // + 키
    [SerializeField] private KeyCode speedDownKey = KeyCode.Minus; // - 키
    [SerializeField] private KeyCode nextPresetKey = KeyCode.RightBracket; // ] 키
    [SerializeField] private KeyCode prevPresetKey = KeyCode.LeftBracket; // [ 키
    
    [Header("상태")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private float originalTimeScale = 1f;
    
    // 싱글톤 패턴
    public static TimeScaleController Instance { get; private set; }
    
    // 이벤트
    public System.Action<float> OnSpeedChanged;
    public System.Action<bool> OnPauseToggled;
    
    // 프로퍼티
    public float CurrentSpeed => useCustomSpeed ? customSpeed : presetSpeeds[currentSpeedIndex];
    public bool IsPaused => isPaused;
    public bool IsCustomSpeed => useCustomSpeed;
    
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
        originalTimeScale = Time.timeScale;
        SetTimeScale(CurrentSpeed);
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// 입력을 처리합니다
    /// </summary>
    private void HandleInput()
    {
        // UI 토글
        if (Input.GetKeyDown(toggleUIKey))
        {
            showUI = !showUI;
        }
        
        // 일시정지 토글
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        
        // 속도 리셋
        if (Input.GetKeyDown(resetSpeedKey))
        {
            ResetToNormalSpeed();
        }
        
        // 속도 증가/감소
        if (Input.GetKey(speedUpKey))
        {
            if (useCustomSpeed)
            {
                SetCustomSpeed(customSpeed + 0.1f);
            }
            else
            {
                IncreaseSpeed();
            }
        }
        
        if (Input.GetKey(speedDownKey))
        {
            if (useCustomSpeed)
            {
                SetCustomSpeed(Mathf.Max(0.1f, customSpeed - 0.1f));
            }
            else
            {
                DecreaseSpeed();
            }
        }
        
        // 프리셋 변경
        if (Input.GetKeyDown(nextPresetKey))
        {
            NextPreset();
        }
        
        if (Input.GetKeyDown(prevPresetKey))
        {
            PreviousPreset();
        }
    }
    
    /// <summary>
    /// 시간 배속을 설정합니다
    /// </summary>
    /// <param name="speed">배속 값 (1.0 = 정상 속도)</param>
    public void SetTimeScale(float speed)
    {
        if (isPaused) return;
        
        speed = Mathf.Max(0.1f, speed); // 최소 0.1배속
        Time.timeScale = speed;
        OnSpeedChanged?.Invoke(speed);
        
        Debug.Log($"시간 배속 설정: {speed}x");
    }
    
    /// <summary>
    /// 일시정지를 토글합니다
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = originalTimeScale;
        }
        
        OnPauseToggled?.Invoke(isPaused);
        Debug.Log($"게임 {(isPaused ? "일시정지" : "재개")}");
    }
    
    /// <summary>
    /// 정상 속도로 리셋합니다
    /// </summary>
    public void ResetToNormalSpeed()
    {
        isPaused = false;
        useCustomSpeed = false;
        currentSpeedIndex = 2; // 1x 속도
        SetTimeScale(1f);
        Debug.Log("속도 리셋: 1x");
    }
    
    /// <summary>
    /// 다음 프리셋으로 변경합니다
    /// </summary>
    public void NextPreset()
    {
        if (currentSpeedIndex < presetSpeeds.Length - 1)
        {
            currentSpeedIndex++;
            useCustomSpeed = false;
            SetTimeScale(presetSpeeds[currentSpeedIndex]);
        }
    }
    
    /// <summary>
    /// 이전 프리셋으로 변경합니다
    /// </summary>
    public void PreviousPreset()
    {
        if (currentSpeedIndex > 0)
        {
            currentSpeedIndex--;
            useCustomSpeed = false;
            SetTimeScale(presetSpeeds[currentSpeedIndex]);
        }
    }
    
    /// <summary>
    /// 속도를 증가시킵니다
    /// </summary>
    public void IncreaseSpeed()
    {
        if (currentSpeedIndex < presetSpeeds.Length - 1)
        {
            NextPreset();
        }
        else
        {
            // 마지막 프리셋을 넘어서면 커스텀 속도로 전환
            useCustomSpeed = true;
            SetCustomSpeed(presetSpeeds[currentSpeedIndex] * 2f);
        }
    }
    
    /// <summary>
    /// 속도를 감소시킵니다
    /// </summary>
    public void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0)
        {
            PreviousPreset();
        }
        else
        {
            // 첫 번째 프리셋보다 낮으면 커스텀 속도로 전환
            useCustomSpeed = true;
            SetCustomSpeed(presetSpeeds[currentSpeedIndex] * 0.5f);
        }
    }
    
    /// <summary>
    /// 커스텀 속도를 설정합니다
    /// </summary>
    /// <param name="speed">설정할 속도</param>
    public void SetCustomSpeed(float speed)
    {
        customSpeed = Mathf.Max(0.1f, speed);
        useCustomSpeed = true;
        SetTimeScale(customSpeed);
    }
    
    /// <summary>
    /// 특정 프리셋으로 설정합니다
    /// </summary>
    /// <param name="index">프리셋 인덱스</param>
    public void SetPreset(int index)
    {
        if (index >= 0 && index < presetSpeeds.Length)
        {
            currentSpeedIndex = index;
            useCustomSpeed = false;
            SetTimeScale(presetSpeeds[index]);
        }
    }
    
    /// <summary>
    /// 프리셋을 추가합니다
    /// </summary>
    /// <param name="speed">추가할 속도</param>
    public void AddPreset(float speed)
    {
        var newPresets = new List<float>(presetSpeeds);
        newPresets.Add(speed);
        newPresets.Sort();
        presetSpeeds = newPresets.ToArray();
    }
    
    /// <summary>
    /// 현재 프리셋을 제거합니다
    /// </summary>
    public void RemoveCurrentPreset()
    {
        if (presetSpeeds.Length > 1)
        {
            var newPresets = new List<float>(presetSpeeds);
            newPresets.RemoveAt(currentSpeedIndex);
            presetSpeeds = newPresets.ToArray();
            
            if (currentSpeedIndex >= presetSpeeds.Length)
            {
                currentSpeedIndex = presetSpeeds.Length - 1;
            }
            
            SetTimeScale(presetSpeeds[currentSpeedIndex]);
        }
    }
    
    private void OnGUI()
    {
        if (!showUI) return;
        
        // UI 스타일 설정
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 12;
        
        // UI 배경
        GUI.Box(new Rect(10, 10, 300, 200), "", style);
        
        // 제목
        GUI.Label(new Rect(20, 20, 280, 20), "시간 배속 컨트롤러", style);
        
        // 현재 상태 표시
        string statusText = isPaused ? "일시정지" : $"배속: {Time.timeScale:F1}x";
        GUI.Label(new Rect(20, 45, 280, 20), statusText, style);
        
        // 프리셋 버튼들
        float buttonWidth = 40f;
        float buttonHeight = 25f;
        float startX = 20f;
        float startY = 70f;
        
        for (int i = 0; i < presetSpeeds.Length; i++)
        {
            float x = startX + (i * (buttonWidth + 5));
            float y = startY;
            
            bool isSelected = !useCustomSpeed && currentSpeedIndex == i;
            bool isCustom = useCustomSpeed && Mathf.Approximately(Time.timeScale, presetSpeeds[i]);
            
            if (isSelected || isCustom)
            {
                GUI.backgroundColor = Color.yellow;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            
            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), $"{presetSpeeds[i]:F1}x", buttonStyle))
            {
                SetPreset(i);
            }
        }
        
        GUI.backgroundColor = Color.white;
        
        // 일시정지 버튼
        if (GUI.Button(new Rect(20, 105, 80, 25), isPaused ? "재개" : "일시정지", buttonStyle))
        {
            TogglePause();
        }
        
        // 리셋 버튼
        if (GUI.Button(new Rect(110, 105, 60, 25), "리셋", buttonStyle))
        {
            ResetToNormalSpeed();
        }
        
        // 커스텀 속도 입력
        GUI.Label(new Rect(20, 135, 100, 20), "커스텀 속도:", style);
        string speedInput = GUI.TextField(new Rect(120, 135, 60, 20), customSpeed.ToString("F1"));
        if (float.TryParse(speedInput, out float newSpeed))
        {
            if (Mathf.Abs(newSpeed - customSpeed) > 0.01f)
            {
                SetCustomSpeed(newSpeed);
            }
        }
        
        // 단축키 안내
        GUI.Label(new Rect(20, 160, 280, 40), 
            "단축키: F1(UI토글) Space(일시정지) R(리셋)\n+/- (속도조절) [/] (프리셋변경)", 
            new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });
    }
    
    private void OnDestroy()
    {
        // 게임 종료 시 정상 속도로 복원
        Time.timeScale = 1f;
    }
}
