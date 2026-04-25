using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dungeon", menuName = "ARPG/Dungeon Config")]
public class DungeonConfig : ScriptableObject
{
    [Header("基本信息")]
    public string dungeonName = "新副本";
    public Sprite dungeonIcon;
    public string description = "副本描述";
    public int dungeonId;
    
    [Header("场景和难度")]
    public string sceneName;
    public int recommendedLevel = 1;
    public DungeonDifficulty difficulty;
    
    [Header("怪物配置")]
    public List<MonsterSpawn> normalMonsters;
    public List<MonsterSpawn> eliteMonsters;
    public MonsterConfig bossMonster;
    
    [Header("奖励")]
    public int completionExperience = 100;
    public int completionGold = 50;
    public DropTableConfig bossDropTable;
    
    [Header("限制")]
    public int maxDeaths = 3;
    public float timeLimit = 600f;
}

public enum DungeonDifficulty
{
    Normal,
    Hard,
    Nightmare,
    Hell
}

[System.Serializable]
public class MonsterSpawn
{
    public MonsterConfig monsterConfig;
    public int minCount = 1;
    public int maxCount = 3;
    public List<Transform> spawnPoints;
}
