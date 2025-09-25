using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HP 바 UI를 관리하는 클래스
/// Slider를 사용하여 HP를 시각적으로 표시
/// </summary>
public class HPBarUI : MonoBehaviour
{
    [Header("HP 바 설정")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI hpText;
    
    [Header("색상 설정")]
    [SerializeField] private Color highHPColor = Color.green;
    [SerializeField] private Color mediumHPColor = Color.yellow;
    [SerializeField] private Color lowHPColor = Color.red;
    [SerializeField] private float highHPThreshold = 0.6f;
    [SerializeField] private float mediumHPThreshold = 0.3f;
    
    [Header("애니메이션 설정")]
    [SerializeField] private bool enableSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 2f;
    
    private Character targetCharacter;
    private float targetHPValue;
    private bool isTransitioning = false;
    
    // 이벤트
    public System.Action OnHPBarEmpty;
    public System.Action OnHPBarFull;
    
    private void Start()
    {
        // HP 슬라이더 초기화
        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = 1f;
        }
        
        // HP 텍스트 초기화
        UpdateHPText(100, 100);
    }
    
    private void Update()
    {
        // 부드러운 전환 애니메이션
        if (isTransitioning && enableSmoothTransition)
        {
            SmoothTransition();
        }
    }
    
    /// <summary>
    /// HP 바에 연결할 캐릭터를 설정합니다
    /// </summary>
    /// <param name="character">연결할 캐릭터</param>
    public void SetTargetCharacter(Character character)
    {
        // 기존 이벤트 구독 해제
        if (targetCharacter != null)
        {
            targetCharacter.OnHPChanged -= OnHPChanged;
        }
        
        targetCharacter = character;
        
        // 새로운 이벤트 구독
        if (targetCharacter != null)
        {
            targetCharacter.OnHPChanged += OnHPChanged;
            
            // 초기 HP 값 설정
            UpdateHPBar(targetCharacter.CurrentHP, targetCharacter.MaxHP);
        }
    }
    
    /// <summary>
    /// HP가 변경되었을 때 호출됩니다
    /// </summary>
    /// <param name="currentHP">현재 HP</param>
    /// <param name="maxHP">최대 HP</param>
    private void OnHPChanged(int currentHP, int maxHP)
    {
        UpdateHPBar(currentHP, maxHP);
    }
    
    /// <summary>
    /// HP 바를 업데이트합니다
    /// </summary>
    /// <param name="currentHP">현재 HP</param>
    /// <param name="maxHP">최대 HP</param>
    public void UpdateHPBar(int currentHP, int maxHP)
    {
        if (maxHP <= 0) return;
        
        float hpPercentage = (float)currentHP / maxHP;
        targetHPValue = hpPercentage;
        
        if (enableSmoothTransition)
        {
            isTransitioning = true;
        }
        else
        {
            hpSlider.value = hpPercentage;
        }
        
        // HP 텍스트 업데이트
        UpdateHPText(currentHP, maxHP);
        
        // HP 바 색상 업데이트
        UpdateHPBarColor(hpPercentage);
        
        // 이벤트 발생
        if (hpPercentage <= 0f)
        {
            OnHPBarEmpty?.Invoke();
        }
        else if (hpPercentage >= 1f)
        {
            OnHPBarFull?.Invoke();
        }
    }
    
    /// <summary>
    /// 부드러운 전환 애니메이션
    /// </summary>
    private void SmoothTransition()
    {
        if (hpSlider == null) return;
        
        float currentValue = hpSlider.value;
        float difference = Mathf.Abs(currentValue - targetHPValue);
        
        if (difference > 0.01f)
        {
            hpSlider.value = Mathf.Lerp(currentValue, targetHPValue, transitionSpeed * Time.deltaTime);
            UpdateHPBarColor(hpSlider.value);
        }
        else
        {
            hpSlider.value = targetHPValue;
            isTransitioning = false;
        }
    }
    
    /// <summary>
    /// HP 바 색상을 업데이트합니다
    /// </summary>
    /// <param name="hpPercentage">HP 비율 (0-1)</param>
    private void UpdateHPBarColor(float hpPercentage)
    {
        if (fillImage == null) return;
        
        Color targetColor;
        
        if (hpPercentage >= highHPThreshold)
        {
            targetColor = highHPColor;
        }
        else if (hpPercentage >= mediumHPThreshold)
        {
            targetColor = mediumHPColor;
        }
        else
        {
            targetColor = lowHPColor;
        }
        
        fillImage.color = targetColor;
    }
    
    /// <summary>
    /// HP 텍스트를 업데이트합니다
    /// </summary>
    /// <param name="currentHP">현재 HP</param>
    /// <param name="maxHP">최대 HP</param>
    private void UpdateHPText(int currentHP, int maxHP)
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHP} / {maxHP}";
        }
    }
    
    /// <summary>
    /// HP 바를 즉시 설정합니다 (애니메이션 없이)
    /// </summary>
    /// <param name="currentHP">현재 HP</param>
    /// <param name="maxHP">최대 HP</param>
    public void SetHPBarImmediate(int currentHP, int maxHP)
    {
        if (maxHP <= 0) return;
        
        float hpPercentage = (float)currentHP / maxHP;
        hpSlider.value = hpPercentage;
        UpdateHPBarColor(hpPercentage);
        UpdateHPText(currentHP, maxHP);
        isTransitioning = false;
    }
    
    /// <summary>
    /// HP 바를 숨깁니다
    /// </summary>
    public void HideHPBar()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// HP 바를 표시합니다
    /// </summary>
    public void ShowHPBar()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// HP 바가 숨겨져 있는지 확인합니다
    /// </summary>
    public bool IsHidden => !gameObject.activeInHierarchy;
    
    /// <summary>
    /// 현재 HP 비율을 반환합니다
    /// </summary>
    public float CurrentHPPercentage => hpSlider != null ? hpSlider.value : 0f;
    
    /// <summary>
    /// HP 바 색상을 직접 설정합니다
    /// </summary>
    /// <param name="color">설정할 색상</param>
    public void SetHPBarColor(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }
    
    /// <summary>
    /// 전환 속도를 설정합니다
    /// </summary>
    /// <param name="speed">새로운 전환 속도</param>
    public void SetTransitionSpeed(float speed)
    {
        transitionSpeed = Mathf.Max(0f, speed);
    }
    
    /// <summary>
    /// 부드러운 전환을 활성화/비활성화합니다
    /// </summary>
    /// <param name="enable">활성화 여부</param>
    public void SetSmoothTransition(bool enable)
    {
        enableSmoothTransition = enable;
        if (!enable)
        {
            isTransitioning = false;
        }
    }
}
