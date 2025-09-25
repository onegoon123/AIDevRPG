using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 시스템을 관리하는 클래스
/// 2D 게임용 자동 전투 시스템
/// </summary>
public class CombatSystem : MonoBehaviour
{
    [Header("2D 전투 설정")]
    [SerializeField] private float globalCombatRange = 10f;
    [SerializeField] private bool enableAutoCombat = true;
    [SerializeField] private float combatCheckInterval = 0.5f;
    
    private List<Character> allCharacters = new List<Character>();
    private float lastCombatCheck;
    
    // 이벤트
    public System.Action<Character, Character, int> OnAttackPerformed; // (공격자, 대상, 피해량)
    public System.Action<Character> OnCharacterDied;
    
    private void Start()
    {
        // 모든 캐릭터를 찾아서 등록
        RegisterAllCharacters();
    }
    
    private void Update()
    {
        if (!enableAutoCombat) return;
        
        // 주기적으로 전투 상태 확인
        if (Time.time - lastCombatCheck >= combatCheckInterval)
        {
            CheckCombatStatus();
            lastCombatCheck = Time.time;
        }
    }
    
    /// <summary>
    /// 모든 캐릭터를 등록합니다
    /// </summary>
    public void RegisterAllCharacters()
    {
        allCharacters.Clear();
        Character[] characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        
        foreach (Character character in characters)
        {
            RegisterCharacter(character);
        }
    }
    
    /// <summary>
    /// 캐릭터를 전투 시스템에 등록합니다
    /// </summary>
    /// <param name="character">등록할 캐릭터</param>
    public void RegisterCharacter(Character character)
    {
        if (character != null && !allCharacters.Contains(character))
        {
            allCharacters.Add(character);
            
            // 캐릭터 사망 이벤트 구독
            character.OnCharacterDied += () => OnCharacterDeath(character);
        }
    }
    
    /// <summary>
    /// 캐릭터를 전투 시스템에서 제거합니다
    /// </summary>
    /// <param name="character">제거할 캐릭터</param>
    public void UnregisterCharacter(Character character)
    {
        if (allCharacters.Contains(character))
        {
            allCharacters.Remove(character);
        }
    }
    
    /// <summary>
    /// 전투 상태를 확인합니다
    /// </summary>
    private void CheckCombatStatus()
    {
        // 죽은 캐릭터들을 제거
        allCharacters.RemoveAll(character => character == null || !character.IsAlive);
        
        // 살아있는 캐릭터들 간의 거리 확인
        for (int i = 0; i < allCharacters.Count; i++)
        {
            for (int j = i + 1; j < allCharacters.Count; j++)
            {
                Character char1 = allCharacters[i];
                Character char2 = allCharacters[j];
                
                if (char1.IsAlive && char2.IsAlive)
                {
                    float distance = Vector2.Distance(char1.transform.position, char2.transform.position);
                    
                    // 전역 전투 범위 내에 있으면 전투 가능
                    if (distance <= globalCombatRange)
                    {
                        // 각 캐릭터가 자동으로 적을 찾아서 공격하도록 함
                        // Character 클래스의 Update에서 자동으로 처리됨
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 캐릭터가 죽었을 때 호출됩니다
    /// </summary>
    /// <param name="character">죽은 캐릭터</param>
    private void OnCharacterDeath(Character character)
    {
        OnCharacterDied?.Invoke(character);
        UnregisterCharacter(character);
        
        Debug.Log($"{character.name}이(가) 전투에서 제거되었습니다.");
    }
    
    /// <summary>
    /// 자동 전투를 활성화/비활성화합니다
    /// </summary>
    /// <param name="enable">활성화 여부</param>
    public void SetAutoCombat(bool enable)
    {
        enableAutoCombat = enable;
    }
    
    /// <summary>
    /// 전역 전투 범위를 설정합니다
    /// </summary>
    /// <param name="range">새로운 전역 전투 범위</param>
    public void SetGlobalCombatRange(float range)
    {
        globalCombatRange = Mathf.Max(0f, range);
    }
    
    /// <summary>
    /// 전투 확인 간격을 설정합니다
    /// </summary>
    /// <param name="interval">새로운 확인 간격</param>
    public void SetCombatCheckInterval(float interval)
    {
        combatCheckInterval = Mathf.Max(0.1f, interval);
    }
    
    /// <summary>
    /// 현재 등록된 캐릭터 수를 반환합니다
    /// </summary>
    public int CharacterCount => allCharacters.Count;
    
    /// <summary>
    /// 특정 타입의 캐릭터 수를 반환합니다
    /// </summary>
    /// <typeparam name="T">캐릭터 타입</typeparam>
    /// <returns>해당 타입의 캐릭터 수</returns>
    public int GetCharacterCount<T>() where T : Character
    {
        int count = 0;
        foreach (Character character in allCharacters)
        {
            if (character is T && character.IsAlive)
            {
                count++;
            }
        }
        return count;
    }
}
