using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 층 전환 UI를 관리하는 클래스
/// 층 클리어 시 전환 애니메이션과 UI를 처리합니다
/// </summary>
public class FloorTransition : MonoBehaviour
{
    [Header("전환 UI")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private TextMeshProUGUI floorClearText;
    [SerializeField] private TextMeshProUGUI nextFloorText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button continueButton;
    
    [Header("전환 설정")]
    [SerializeField] private float transitionDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Color transitionColor = Color.black;
    
    [Header("애니메이션 설정")]
    [SerializeField] private bool enableCountdown = true;
    [SerializeField] private bool enableFadeEffect = true;
    [SerializeField] private bool enableScaleAnimation = true;
    
    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;
    
    // 이벤트
    public System.Action OnTransitionComplete;
    public System.Action OnTransitionStart;
    
    private void Start()
    {
        // 초기 상태 설정
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }
        
        // 계속 버튼 이벤트 연결
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
    }
    
    /// <summary>
    /// 층 전환을 시작합니다
    /// </summary>
    /// <param name="currentFloor">현재 층</param>
    /// <param name="nextFloor">다음 층</param>
    public void StartFloorTransition(int currentFloor, int nextFloor)
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        OnTransitionStart?.Invoke();
        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(TransitionCoroutine(currentFloor, nextFloor));
    }
    
    /// <summary>
    /// 던전 완료 전환을 시작합니다
    /// </summary>
    public void StartDungeonCompleteTransition()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        OnTransitionStart?.Invoke();
        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(DungeonCompleteCoroutine());
    }
    
    /// <summary>
    /// 층 전환 코루틴
    /// </summary>
    private IEnumerator TransitionCoroutine(int currentFloor, int nextFloor)
    {
        // 전환 패널 표시
        ShowTransitionPanel();
        UpdateTransitionText(currentFloor, nextFloor);
        
        // 페이드 인
        if (enableFadeEffect)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        // 스케일 애니메이션
        if (enableScaleAnimation)
        {
            yield return StartCoroutine(ScaleAnimation());
        }
        
        // 카운트다운
        if (enableCountdown)
        {
            yield return StartCoroutine(CountdownCoroutine());
        }
        else
        {
            yield return new WaitForSeconds(transitionDuration);
        }
        
        // 페이드 아웃
        if (enableFadeEffect)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // 전환 완료
        CompleteTransition();
    }
    
    /// <summary>
    /// 던전 완료 코루틴
    /// </summary>
    private IEnumerator DungeonCompleteCoroutine()
    {
        // 전환 패널 표시
        ShowTransitionPanel();
        UpdateDungeonCompleteText();
        
        // 페이드 인
        if (enableFadeEffect)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        // 스케일 애니메이션
        if (enableScaleAnimation)
        {
            yield return StartCoroutine(ScaleAnimation());
        }
        
        // 대기
        yield return new WaitForSeconds(transitionDuration);
        
        // 페이드 아웃
        if (enableFadeEffect)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // 전환 완료
        CompleteTransition();
    }
    
    /// <summary>
    /// 전환 패널을 표시합니다
    /// </summary>
    private void ShowTransitionPanel()
    {
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 전환 텍스트를 업데이트합니다
    /// </summary>
    private void UpdateTransitionText(int currentFloor, int nextFloor)
    {
        if (floorClearText != null)
        {
            floorClearText.text = $"{currentFloor}층 클리어!";
        }
        
        if (nextFloorText != null)
        {
            nextFloorText.text = $"다음 층: {nextFloor}층";
        }
    }
    
    /// <summary>
    /// 던전 완료 텍스트를 업데이트합니다
    /// </summary>
    private void UpdateDungeonCompleteText()
    {
        if (floorClearText != null)
        {
            floorClearText.text = "던전 완료!";
        }
        
        if (nextFloorText != null)
        {
            nextFloorText.text = "축하합니다!";
        }
    }
    
    /// <summary>
    /// 페이드 인 애니메이션
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (backgroundImage == null) yield break;
        
        Color startColor = transitionColor;
        startColor.a = 0f;
        Color endColor = transitionColor;
        
        backgroundImage.color = startColor;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            
            Color currentColor = transitionColor;
            currentColor.a = alpha;
            backgroundImage.color = currentColor;
            
            yield return null;
        }
        
        backgroundImage.color = endColor;
    }
    
    /// <summary>
    /// 페이드 아웃 애니메이션
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (backgroundImage == null) yield break;
        
        Color startColor = transitionColor;
        Color endColor = transitionColor;
        endColor.a = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            
            Color currentColor = transitionColor;
            currentColor.a = alpha;
            backgroundImage.color = currentColor;
            
            yield return null;
        }
        
        backgroundImage.color = endColor;
    }
    
    /// <summary>
    /// 스케일 애니메이션
    /// </summary>
    private IEnumerator ScaleAnimation()
    {
        if (transitionPanel == null) yield break;
        
        Vector3 originalScale = transitionPanel.transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        // 스케일 업
        float elapsedTime = 0f;
        float scaleDuration = 0.3f;
        
        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.1f, elapsedTime / scaleDuration);
            transitionPanel.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // 스케일 다운
        elapsedTime = 0f;
        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1.1f, 1f, elapsedTime / scaleDuration);
            transitionPanel.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transitionPanel.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 카운트다운 코루틴
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        if (countdownText == null) yield break;
        
        int countdown = Mathf.RoundToInt(transitionDuration);
        
        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        
        countdownText.text = "시작!";
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 전환을 완료합니다
    /// </summary>
    private void CompleteTransition()
    {
        isTransitioning = false;
        
        // 전환 패널 숨기기
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }
        
        // 이벤트 발생
        OnTransitionComplete?.Invoke();
        
        Debug.Log("층 전환 완료");
    }
    
    /// <summary>
    /// 계속 버튼 클릭 시 호출됩니다
    /// </summary>
    private void OnContinueButtonClicked()
    {
        if (isTransitioning)
        {
            // 전환을 즉시 완료
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            CompleteTransition();
        }
    }
    
    /// <summary>
    /// 전환 중인지 확인합니다
    /// </summary>
    public bool IsTransitioning => isTransitioning;
    
    /// <summary>
    /// 전환 설정을 업데이트합니다
    /// </summary>
    /// <param name="duration">전환 지속 시간</param>
    /// <param name="fadeIn">페이드 인 지속 시간</param>
    /// <param name="fadeOut">페이드 아웃 지속 시간</param>
    public void SetTransitionSettings(float duration, float fadeIn, float fadeOut)
    {
        transitionDuration = duration;
        fadeInDuration = fadeIn;
        fadeOutDuration = fadeOut;
    }
    
    /// <summary>
    /// 전환 색상을 설정합니다
    /// </summary>
    /// <param name="color">새로운 색상</param>
    public void SetTransitionColor(Color color)
    {
        transitionColor = color;
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
}
