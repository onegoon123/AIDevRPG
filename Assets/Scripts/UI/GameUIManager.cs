using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 UI를 관리하는 매니저 클래스
/// 플레이어 HP, 경험치, 레벨 등의 UI를 통합 관리
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("플레이어 UI")]
    [SerializeField] private HPBarUI playerHPBar;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    
    [Header("스킬 UI")]
    [SerializeField] private Button healingSkillButton;
    [SerializeField] private Button criticalSkillButton;
    [SerializeField] private Button damageSkillButton;
    [SerializeField] private TextMeshProUGUI healingSkillLevelText;
    [SerializeField] private TextMeshProUGUI criticalSkillLevelText;
    [SerializeField] private TextMeshProUGUI damageSkillLevelText;
    
    [Header("게임 상태 UI")]
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    
    private PlayerCharacter playerCharacter;
    private bool isUISetup = false;
    
    // 이벤트
    public System.Action OnGameRestart;
    public System.Action OnGameOver;
    
    private void Start()
    {
        SetupUI();
        FindPlayerCharacter();
    }
    
    private void Update()
    {
        if (playerCharacter != null && isUISetup)
        {
            UpdateUI();
        }
    }
    
    /// <summary>
    /// UI를 초기 설정합니다
    /// </summary>
    private void SetupUI()
    {
        // 스킬 버튼 이벤트 연결
        if (healingSkillButton != null)
            healingSkillButton.onClick.AddListener(() => UpgradeSkill(SkillType.Healing));
        
        if (criticalSkillButton != null)
            criticalSkillButton.onClick.AddListener(() => UpgradeSkill(SkillType.CriticalChance));
        
        if (damageSkillButton != null)
            damageSkillButton.onClick.AddListener(() => UpgradeSkill(SkillType.DamageBoost));
        
        // 재시작 버튼 이벤트 연결
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        // 게임 오버 패널 숨기기
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        isUISetup = true;
    }
    
    /// <summary>
    /// 플레이어 캐릭터를 찾습니다
    /// </summary>
    private void FindPlayerCharacter()
    {
        PlayerCharacter[] players = FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);
        if (players.Length > 0)
        {
            SetPlayerCharacter(players[0]);
        }
        else
        {
            Debug.LogWarning("플레이어 캐릭터를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 플레이어 캐릭터를 설정합니다
    /// </summary>
    /// <param name="character">플레이어 캐릭터</param>
    public void SetPlayerCharacter(PlayerCharacter character)
    {
        // 기존 이벤트 구독 해제
        if (playerCharacter != null)
        {
            playerCharacter.OnCharacterDied -= OnPlayerDied;
        }
        
        playerCharacter = character;
        
        // 새로운 이벤트 구독
        if (playerCharacter != null)
        {
            playerCharacter.OnCharacterDied += OnPlayerDied;
            
            // HP 바 연결
            if (playerHPBar != null)
            {
                playerHPBar.SetTargetCharacter(playerCharacter);
            }
        }
    }
    
    /// <summary>
    /// UI를 업데이트합니다
    /// </summary>
    private void UpdateUI()
    {
        UpdateExperienceUI();
        UpdateSkillUI();
        UpdateGameStatusUI();
    }
    
    /// <summary>
    /// 경험치 UI를 업데이트합니다
    /// </summary>
    private void UpdateExperienceUI()
    {
        if (playerCharacter == null) return;
        
        // 레벨 텍스트 업데이트
        if (levelText != null)
        {
            levelText.text = $"Level {playerCharacter.Level}";
        }
        
        // 경험치 바 업데이트
        if (experienceBar != null)
        {
            float expPercentage = (float)playerCharacter.Experience / playerCharacter.ExperienceToNextLevel;
            experienceBar.value = expPercentage;
        }
        
        // 경험치 텍스트 업데이트
        if (experienceText != null)
        {
            experienceText.text = $"{playerCharacter.Experience} / {playerCharacter.ExperienceToNextLevel}";
        }
        
        // 스킬 포인트 텍스트 업데이트
        if (skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points: {playerCharacter.SkillPoints}";
        }
    }
    
    /// <summary>
    /// 스킬 UI를 업데이트합니다
    /// </summary>
    private void UpdateSkillUI()
    {
        if (playerCharacter == null) return;
        
        // 스킬 레벨 텍스트 업데이트
        if (healingSkillLevelText != null)
        {
            healingSkillLevelText.text = $"Lv.{playerCharacter.GetSkillLevel(SkillType.Healing)}";
        }
        
        if (criticalSkillLevelText != null)
        {
            criticalSkillLevelText.text = $"Lv.{playerCharacter.GetSkillLevel(SkillType.CriticalChance)}";
        }
        
        if (damageSkillLevelText != null)
        {
            damageSkillLevelText.text = $"Lv.{playerCharacter.GetSkillLevel(SkillType.DamageBoost)}";
        }
        
        // 스킬 포인트가 있을 때만 버튼 활성화
        bool canUpgrade = playerCharacter.SkillPoints > 0;
        
        if (healingSkillButton != null)
            healingSkillButton.interactable = canUpgrade;
        
        if (criticalSkillButton != null)
            criticalSkillButton.interactable = canUpgrade;
        
        if (damageSkillButton != null)
            damageSkillButton.interactable = canUpgrade;
    }
    
    /// <summary>
    /// 게임 상태 UI를 업데이트합니다
    /// </summary>
    private void UpdateGameStatusUI()
    {
        if (playerCharacter == null) return;
        
        if (gameStatusText != null)
        {
            string status = $"HP: {playerCharacter.CurrentHP}/{playerCharacter.MaxHP} | " +
                          $"Level: {playerCharacter.Level} | " +
                          $"Exp: {playerCharacter.Experience}/{playerCharacter.ExperienceToNextLevel}";
            gameStatusText.text = status;
        }
    }
    
    /// <summary>
    /// 스킬을 업그레이드합니다
    /// </summary>
    /// <param name="skillType">업그레이드할 스킬 타입</param>
    private void UpgradeSkill(SkillType skillType)
    {
        if (playerCharacter == null) return;
        
        bool success = playerCharacter.UpgradeSkill(skillType);
        if (success)
        {
            Debug.Log($"{skillType} 스킬이 업그레이드되었습니다!");
        }
        else
        {
            Debug.Log("스킬 포인트가 부족합니다.");
        }
    }
    
    /// <summary>
    /// 플레이어가 죽었을 때 호출됩니다
    /// </summary>
    private void OnPlayerDied()
    {
        ShowGameOver();
        OnGameOver?.Invoke();
    }
    
    /// <summary>
    /// 게임 오버 화면을 표시합니다
    /// </summary>
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Debug.Log("게임 오버!");
    }
    
    /// <summary>
    /// 게임을 재시작합니다
    /// </summary>
    private void RestartGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 플레이어 부활
        if (playerCharacter != null)
        {
            playerCharacter.Revive();
        }
        
        OnGameRestart?.Invoke();
        Debug.Log("게임이 재시작되었습니다.");
    }
    
    /// <summary>
    /// HP 바를 표시/숨깁니다
    /// </summary>
    /// <param name="show">표시 여부</param>
    public void SetHPBarVisible(bool show)
    {
        if (playerHPBar != null)
        {
            if (show)
                playerHPBar.ShowHPBar();
            else
                playerHPBar.HideHPBar();
        }
    }
    
    /// <summary>
    /// 경험치 바를 표시/숨깁니다
    /// </summary>
    /// <param name="show">표시 여부</param>
    public void SetExperienceBarVisible(bool show)
    {
        if (experienceBar != null)
        {
            experienceBar.gameObject.SetActive(show);
        }
    }
    
    /// <summary>
    /// 스킬 UI를 표시/숨깁니다
    /// </summary>
    /// <param name="show">표시 여부</param>
    public void SetSkillUIVisible(bool show)
    {
        if (healingSkillButton != null)
            healingSkillButton.gameObject.SetActive(show);
        
        if (criticalSkillButton != null)
            criticalSkillButton.gameObject.SetActive(show);
        
        if (damageSkillButton != null)
            damageSkillButton.gameObject.SetActive(show);
    }
}
