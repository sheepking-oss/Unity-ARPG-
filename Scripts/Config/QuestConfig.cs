using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Quest", menuName = "ARPG/Quest Config")]
public class QuestConfig : ScriptableObject
{
    [Header("基本信息")]
    public string questName = "新任务";
    public string description = "任务描述";
    public int questId;
    
    [Header("任务类型")]
    public QuestType questType;
    
    [Header("任务要求")]
    public List<QuestRequirement> requirements;
    public int requiredLevel = 1;
    
    [Header("任务奖励")]
    public int experienceReward = 100;
    public int goldReward = 50;
    public List<EquipmentReward> equipmentRewards;
    
    [Header("前置任务")]
    public List<int> prerequisiteQuestIds;
    
    [Header("对话")]
    public string startDialogue;
    public string progressDialogue;
    public string completeDialogue;
}

public enum QuestType
{
    Kill,
    Collect,
    Talk,
    Explore,
    Escort
}

[System.Serializable]
public class QuestRequirement
{
    public QuestRequirementType type;
    public int targetId;
    public string targetName;
    public int requiredAmount;
    public int currentAmount;
}

public enum QuestRequirementType
{
    KillMonster,
    CollectItem,
    TalkToNPC,
    ReachLocation,
    ClearDungeon
}

[System.Serializable]
public class EquipmentReward
{
    public int equipmentId;
    [Range(0f, 1f)]
    public float dropChance = 1f;
}
