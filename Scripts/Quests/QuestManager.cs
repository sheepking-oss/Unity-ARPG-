using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [Header("任务配置")]
    public List<QuestConfig> allQuests;
    
    [Header("UI引用")]
    public QuestUI questUI;
    
    private Dictionary<int, QuestConfig> questDictionary;
    private CharacterData playerData;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        InitializeQuestDictionary();
    }
    
    private void Start()
    {
        if (CharacterManager.Instance != null)
        {
            playerData = CharacterManager.Instance.playerCharacter;
        }
    }
    
    private void InitializeQuestDictionary()
    {
        questDictionary = new Dictionary<int, QuestConfig>();
        
        foreach (var quest in allQuests)
        {
            if (!questDictionary.ContainsKey(quest.questId))
            {
                questDictionary.Add(quest.questId, quest);
            }
            else
            {
                Debug.LogWarning($"重复的任务ID：{quest.questId}");
            }
        }
    }
    
    public QuestConfig GetQuestById(int id)
    {
        if (questDictionary.TryGetValue(id, out QuestConfig config))
        {
            return config;
        }
        return null;
    }
    
    public bool CanAcceptQuest(QuestConfig quest)
    {
        if (quest == null) return false;
        
        if (playerData == null) return false;
        
        if (playerData.level < quest.requiredLevel)
        {
            return false;
        }
        
        foreach (var prerequisiteId in quest.prerequisiteQuestIds)
        {
            bool prerequisiteCompleted = false;
            
            foreach (var completedQuest in playerData.completedQuests)
            {
                if (completedQuest.config.questId == prerequisiteId)
                {
                    prerequisiteCompleted = true;
                    break;
                }
            }
            
            if (!prerequisiteCompleted)
            {
                return false;
            }
        }
        
        foreach (var activeQuest in playerData.activeQuests)
        {
            if (activeQuest.config.questId == quest.questId)
            {
                return false;
            }
        }
        
        foreach (var completedQuest in playerData.completedQuests)
        {
            if (completedQuest.config.questId == quest.questId)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public bool AcceptQuest(QuestConfig quest)
    {
        if (!CanAcceptQuest(quest))
        {
            return false;
        }
        
        QuestInstance questInstance = new QuestInstance
        {
            config = quest,
            status = QuestStatus.InProgress,
            currentProgress = 0
        };
        
        playerData.activeQuests.Add(questInstance);
        
        Debug.Log($"接受了任务：{quest.questName}");
        
        UpdateQuestUI();
        
        return true;
    }
    
    public void UpdateQuestProgress(QuestRequirementType type, int targetId, int amount = 1)
    {
        if (playerData == null) return;
        
        foreach (var questInstance in playerData.activeQuests)
        {
            if (questInstance.status != QuestStatus.InProgress) continue;
            
            foreach (var requirement in questInstance.config.requirements)
            {
                if (requirement.type == type && requirement.targetId == targetId)
                {
                    requirement.currentAmount += amount;
                    
                    Debug.Log($"任务进度更新：{questInstance.config.questName} - {requirement.targetName} ({requirement.currentAmount}/{requirement.requiredAmount})");
                    
                    CheckQuestCompletion(questInstance);
                }
            }
        }
        
        UpdateQuestUI();
    }
    
    public void CheckQuestCompletion(QuestInstance questInstance)
    {
        bool allRequirementsMet = true;
        
        foreach (var requirement in questInstance.config.requirements)
        {
            if (requirement.currentAmount < requirement.requiredAmount)
            {
                allRequirementsMet = false;
                break;
            }
        }
        
        if (allRequirementsMet)
        {
            questInstance.status = QuestStatus.Completed;
            Debug.Log($"任务完成：{questInstance.config.questName}");
        }
    }
    
    public bool TurnInQuest(QuestInstance questInstance)
    {
        if (questInstance.status != QuestStatus.Completed)
        {
            return false;
        }
        
        GiveQuestRewards(questInstance.config);
        
        questInstance.status = QuestStatus.TurnedIn;
        playerData.activeQuests.Remove(questInstance);
        playerData.completedQuests.Add(questInstance);
        
        Debug.Log($"提交了任务：{questInstance.config.questName}");
        
        UpdateQuestUI();
        
        return true;
    }
    
    private void GiveQuestRewards(QuestConfig quest)
    {
        if (CharacterManager.Instance == null) return;
        
        if (quest.experienceReward > 0)
        {
            CharacterManager.Instance.AddExperience(quest.experienceReward);
            Debug.Log($"获得经验值：{quest.experienceReward}");
        }
        
        if (quest.goldReward > 0)
        {
            CharacterManager.Instance.AddGold(quest.goldReward);
            Debug.Log($"获得金币：{quest.goldReward}");
        }
        
        foreach (var equipmentReward in quest.equipmentRewards)
        {
            if (Random.value < equipmentReward.dropChance)
            {
                EquipmentInstance equipment = EquipmentGenerator.Instance?.GenerateEquipment(equipmentReward.equipmentId);
                if (equipment != null)
                {
                    CharacterManager.Instance.AddToInventory(equipment);
                    Debug.Log($"获得装备：{equipment.config.equipmentName}");
                }
            }
        }
    }
    
    public List<QuestInstance> GetActiveQuests()
    {
        if (playerData == null) return new List<QuestInstance>();
        return playerData.activeQuests;
    }
    
    public List<QuestInstance> GetCompletedQuests()
    {
        if (playerData == null) return new List<QuestInstance>();
        return playerData.completedQuests;
    }
    
    public void OnMonsterKilled(int monsterId)
    {
        UpdateQuestProgress(QuestRequirementType.KillMonster, monsterId);
    }
    
    public void OnItemCollected(int itemId)
    {
        UpdateQuestProgress(QuestRequirementType.CollectItem, itemId);
    }
    
    public void OnNPCTalked(int npcId)
    {
        UpdateQuestProgress(QuestRequirementType.TalkToNPC, npcId);
    }
    
    public void OnDungeonCleared(int dungeonId)
    {
        UpdateQuestProgress(QuestRequirementType.ClearDungeon, dungeonId);
    }
    
    private void UpdateQuestUI()
    {
        if (questUI != null)
        {
            questUI.UpdateQuestList();
        }
    }
}
