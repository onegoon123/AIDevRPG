using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 데이터 구조체
/// 각 층의 정보를 담고 있습니다
/// </summary>
[System.Serializable]
public struct DungeonFloorData
{
    [Header("층 정보")]
    public int floorNumber;
    public string floorName;
    public string floorDescription;
    
    [Header("적 스폰 데이터")]
    public List<EnemySpawnData> enemySpawns;
    
    [Header("보상 데이터")]
    public int experienceReward;
    public int goldReward;
    public List<ItemReward> itemRewards;
    
    [Header("층 설정")]
    public float floorClearTime;
    public bool isBossFloor;
    public string bossName;
}

/// <summary>
/// 적 스폰 데이터
/// </summary>
[System.Serializable]
public struct EnemySpawnData
{
    public EnemyType enemyType;
    public int spawnCount;
    public Vector2 spawnPosition;
    public float spawnDelay;
    public bool isBoss;
}

/// <summary>
/// 아이템 보상 데이터
/// </summary>
[System.Serializable]
public struct ItemReward
{
    public string itemName;
    public int itemCount;
    public float dropChance;
}

/// <summary>
/// 던전 설정 데이터
/// </summary>
[CreateAssetMenu(fileName = "DungeonData", menuName = "Dungeon/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    [Header("던전 기본 정보")]
    public string dungeonName;
    public string dungeonDescription;
    public Sprite dungeonIcon;
    
    [Header("던전 설정")]
    public int totalFloors = 10;
    public bool isInfinite = false;
    public float difficultyMultiplier = 1.0f;
    
    [Header("층별 데이터")]
    public List<DungeonFloorData> floorData;
    
    [Header("던전 보상")]
    public int baseExperienceReward = 100;
    public int baseGoldReward = 50;
    public List<ItemReward> completionRewards;
    
    /// <summary>
    /// 특정 층의 데이터를 가져옵니다
    /// </summary>
    /// <param name="floorNumber">층 번호</param>
    /// <returns>층 데이터</returns>
    public DungeonFloorData GetFloorData(int floorNumber)
    {
        if (floorNumber < 1 || floorNumber > floorData.Count)
        {
            Debug.LogWarning($"존재하지 않는 층입니다: {floorNumber}");
            return new DungeonFloorData();
        }
        
        return floorData[floorNumber - 1];
    }
    
    /// <summary>
    /// 총 층 수를 반환합니다
    /// </summary>
    public int GetTotalFloors()
    {
        return floorData.Count;
    }
    
    /// <summary>
    /// 던전이 무한인지 확인합니다
    /// </summary>
    public bool IsInfiniteDungeon()
    {
        return isInfinite;
    }
    
    /// <summary>
    /// 난이도 배수를 반환합니다
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
    }
}
