using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 로그 데이터 구조체들
/// </summary>

[System.Serializable]
public class GameSessionData
{
    public string sessionId;
    public DateTime startTime;
    public DateTime endTime;
    public float totalPlayTime;
    public int totalFloorsCleared;
    public bool isCompleted;
    public string completionReason; // "Victory", "Defeat", "Exit"
}

[System.Serializable]
public class PlayerActionData
{
    public DateTime timestamp;
    public string actionType; // "Attack", "Heal", "LevelUp", "SkillUpgrade", "Move"
    public string targetName;
    public Vector3 position;
    public int value; // 데미지, 힐량, 경험치 등
    public string additionalInfo;
}

[System.Serializable]
public class CombatData
{
    public DateTime timestamp;
    public string attackerName;
    public string targetName;
    public int damageDealt;
    public bool isCritical;
    public int attackerHP;
    public int targetHP;
    public string combatResult; // "Hit", "Miss", "Critical"
}

[System.Serializable]
public class DungeonProgressData
{
    public DateTime timestamp;
    public int floorNumber;
    public string floorName;
    public int enemiesSpawned;
    public int enemiesKilled;
    public float timeSpentOnFloor;
    public string floorResult; // "Cleared", "Failed", "InProgress"
}

[System.Serializable]
public class EnemySpawnLogData
{
    public DateTime timestamp;
    public string enemyType;
    public Vector3 spawnPosition;
    public int enemyLevel;
    public bool isBoss;
}

[System.Serializable]
public class PlayerStatsData
{
    public DateTime timestamp;
    public int level;
    public int experience;
    public int maxHP;
    public int currentHP;
    public int attackPower;
    public int defense;
    public int skillPoints;
    public int healingSkill;
    public int criticalChance;
    public int damageBoost;
}

[System.Serializable]
public class GameLogData
{
    public GameSessionData sessionData;
    public List<PlayerActionData> playerActions = new List<PlayerActionData>();
    public List<CombatData> combatEvents = new List<CombatData>();
    public List<DungeonProgressData> dungeonProgress = new List<DungeonProgressData>();
    public List<EnemySpawnLogData> enemySpawns = new List<EnemySpawnLogData>();
    public List<PlayerStatsData> playerStats = new List<PlayerStatsData>();
    
    public void AddPlayerAction(string actionType, string targetName, Vector3 position, int value, string additionalInfo = "")
    {
        var now = DateTime.Now;
        float gameTime = Time.time; // 정속도 기준 게임 시간 (배속 무관)
        playerActions.Add(new PlayerActionData
        {
            timestamp = now,
            actionType = actionType,
            targetName = targetName,
            position = position,
            value = value,
            additionalInfo = $"GameTime: {gameTime:F2}s - {additionalInfo}"
        });
    }
    
    public void AddCombatEvent(string attackerName, string targetName, int damageDealt, bool isCritical, int attackerHP, int targetHP, string combatResult)
    {
        var now = DateTime.Now;
        float gameTime = Time.time; // 정속도 기준 게임 시간 (배속 무관)
        combatEvents.Add(new CombatData
        {
            timestamp = now,
            attackerName = attackerName,
            targetName = targetName,
            damageDealt = damageDealt,
            isCritical = isCritical,
            attackerHP = attackerHP,
            targetHP = targetHP,
            combatResult = $"GameTime: {gameTime:F2}s - {combatResult}"
        });
    }
    
    public void AddDungeonProgress(int floorNumber, string floorName, int enemiesSpawned, int enemiesKilled, float timeSpent, string result)
    {
        var now = DateTime.Now;
        float gameTime = Time.time; // 정속도 기준 게임 시간 (배속 무관)
        dungeonProgress.Add(new DungeonProgressData
        {
            timestamp = now,
            floorNumber = floorNumber,
            floorName = floorName,
            enemiesSpawned = enemiesSpawned,
            enemiesKilled = enemiesKilled,
            timeSpentOnFloor = timeSpent,
            floorResult = $"GameTime: {gameTime:F2}s - {result}"
        });
    }
    
    public void AddEnemySpawn(string enemyType, Vector3 spawnPosition, int enemyLevel, bool isBoss)
    {
        var now = DateTime.Now;
        float gameTime = Time.time; // 정속도 기준 게임 시간 (배속 무관)
        enemySpawns.Add(new EnemySpawnLogData
        {
            timestamp = now,
            enemyType = $"GameTime: {gameTime:F2}s - {enemyType}",
            spawnPosition = spawnPosition,
            enemyLevel = enemyLevel,
            isBoss = isBoss
        });
    }
    
    public void AddPlayerStats(PlayerCharacter player)
    {
        var now = DateTime.Now;
        playerStats.Add(new PlayerStatsData
        {
            timestamp = now,
            level = player.Level,
            experience = player.Experience,
            maxHP = player.MaxHP,
            currentHP = player.CurrentHP,
            attackPower = player.AttackPower,
            defense = player.Defense,
            skillPoints = player.SkillPoints,
            healingSkill = player.GetSkillLevel(SkillType.Healing),
            criticalChance = player.GetSkillLevel(SkillType.CriticalChance),
            damageBoost = player.GetSkillLevel(SkillType.DamageBoost)
        });
    }
}
