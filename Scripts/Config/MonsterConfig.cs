using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Monster", menuName = "ARPG/Monster Config")]
public class MonsterConfig : ScriptableObject
{
    [Header("基本信息")]
    public string monsterName = "新怪物";
    public Sprite monsterIcon;
    public GameObject monsterPrefab;
    public int monsterId;
    
    [Header("怪物类型")]
    public MonsterType monsterType;
    public bool isBoss = false;
    
    [Header("基础属性")]
    public int level = 1;
    public CharacterStats stats;
    
    [Header("战斗属性")]
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float detectionRange = 10f;
    public float moveSpeed = 3f;
    
    [Header("经验和金币")]
    public int experienceReward = 50;
    public int goldReward = 10;
    
    [Header("掉落表")]
    public DropTableConfig dropTable;
    
    [Header("AI行为")]
    public MonsterBehavior behavior;
    
    [Header("Boss阶段 (仅Boss)")]
    public List<BossPhase> bossPhases;
}

public enum MonsterType
{
    Normal,
    Elite,
    Boss
}

public enum MonsterBehavior
{
    Aggressive,
    Passive,
    Patrol
}

[System.Serializable]
public class BossPhase
{
    [Range(0f, 1f)]
    public float healthThreshold;
    public string phaseName;
    public List<SkillConfig> phaseSkills;
    public float phaseAttackSpeedMultiplier = 1f;
    public float phaseDamageMultiplier = 1f;
    public GameObject phaseEffect;
}
