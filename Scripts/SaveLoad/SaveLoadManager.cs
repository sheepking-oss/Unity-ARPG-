using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    
    [Header("存档设置")]
    public string saveFileName = "savegame.dat";
    public bool autoSave = true;
    public float autoSaveInterval = 300f;
    
    private float autoSaveTimer = 0f;
    private string savePath;
    
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
        
        savePath = Application.persistentDataPath + "/" + saveFileName;
    }
    
    private void Update()
    {
        if (autoSave)
        {
            autoSaveTimer += Time.deltaTime;
            
            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveGame();
                autoSaveTimer = 0f;
            }
        }
    }
    
    public void SaveGame()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogWarning("没有角色数据可保存！");
            return;
        }
        
        SaveData saveData = new SaveData();
        
        SaveCharacterData(saveData);
        SaveEquipmentData(saveData);
        SaveQuestData(saveData);
        
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Create(savePath);
            formatter.Serialize(file, saveData);
            file.Close();
            
            Debug.Log($"游戏已保存到：{savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存失败：{e.Message}");
        }
    }
    
    public bool LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("没有找到存档文件！");
            return false;
        }
        
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);
            SaveData saveData = (SaveData)formatter.Deserialize(file);
            file.Close();
            
            LoadCharacterData(saveData);
            LoadEquipmentData(saveData);
            LoadQuestData(saveData);
            
            Debug.Log($"游戏已从 {savePath} 加载");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载失败：{e.Message}");
            return false;
        }
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }
    
    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("存档文件已删除");
        }
    }
    
    private void SaveCharacterData(SaveData saveData)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.playerCharacter == null) return;
        
        CharacterData character = CharacterManager.Instance.playerCharacter;
        
        saveData.characterName = character.characterName;
        saveData.classId = character.classConfig != null ? GetClassId(character.classConfig) : -1;
        saveData.level = character.level;
        saveData.experience = character.experience;
        saveData.experienceToNextLevel = character.experienceToNextLevel;
        
        saveData.currentHealth = character.currentHealth;
        saveData.currentMana = character.currentMana;
        saveData.gold = character.gold;
        
        saveData.baseStats = character.baseStats;
        saveData.totalStats = character.totalStats;
        
        saveData.skillLevels = new List<int>();
        foreach (var skill in character.skills)
        {
            saveData.skillLevels.Add(skill.level);
        }
    }
    
    private void SaveEquipmentData(SaveData saveData)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.playerCharacter == null) return;
        
        CharacterData character = CharacterManager.Instance.playerCharacter;
        
        saveData.equippedItemIds = new List<int>();
        saveData.equippedEnhanceLevels = new List<int>();
        
        foreach (var slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            EquipmentSlot equipmentSlot = (EquipmentSlot)slot;
            
            if (character.equippedItems.TryGetValue(equipmentSlot, out EquipmentInstance item) && item != null)
            {
                saveData.equippedItemIds.Add(item.config.equipmentId);
                saveData.equippedEnhanceLevels.Add(item.enhanceLevel);
            }
            else
            {
                saveData.equippedItemIds.Add(-1);
                saveData.equippedEnhanceLevels.Add(0);
            }
        }
        
        saveData.inventoryItemIds = new List<int>();
        saveData.inventoryEnhanceLevels = new List<int>();
        
        foreach (var item in character.inventory)
        {
            if (item != null)
            {
                saveData.inventoryItemIds.Add(item.config.equipmentId);
                saveData.inventoryEnhanceLevels.Add(item.enhanceLevel);
            }
        }
    }
    
    private void SaveQuestData(SaveData saveData)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.playerCharacter == null) return;
        
        CharacterData character = CharacterManager.Instance.playerCharacter;
        
        saveData.activeQuestIds = new List<int>();
        saveData.activeQuestProgress = new List<int>();
        saveData.activeQuestStatus = new List<int>();
        
        foreach (var quest in character.activeQuests)
        {
            if (quest != null && quest.config != null)
            {
                saveData.activeQuestIds.Add(quest.config.questId);
                saveData.activeQuestProgress.Add(quest.currentProgress);
                saveData.activeQuestStatus.Add((int)quest.status);
            }
        }
        
        saveData.completedQuestIds = new List<int>();
        
        foreach (var quest in character.completedQuests)
        {
            if (quest != null && quest.config != null)
            {
                saveData.completedQuestIds.Add(quest.config.questId);
            }
        }
    }
    
    private void LoadCharacterData(SaveData saveData)
    {
        if (CharacterManager.Instance == null) return;
        
        ClassConfig classConfig = GetClassById(saveData.classId);
        if (classConfig == null)
        {
            Debug.LogError("无法找到职业配置！");
            return;
        }
        
        CharacterManager.Instance.SelectClass(classConfig);
        CharacterManager.Instance.CreateCharacter();
        
        if (CharacterManager.Instance.playerCharacter != null)
        {
            CharacterData character = CharacterManager.Instance.playerCharacter;
            
            character.characterName = saveData.characterName;
            character.level = saveData.level;
            character.experience = saveData.experience;
            character.experienceToNextLevel = saveData.experienceToNextLevel;
            
            character.currentHealth = saveData.currentHealth;
            character.currentMana = saveData.currentMana;
            character.gold = saveData.gold;
            
            character.baseStats = saveData.baseStats;
            character.totalStats = saveData.totalStats;
            
            for (int i = 0; i < saveData.skillLevels.Count && i < character.skills.Count; i++)
            {
                character.skills[i].level = saveData.skillLevels[i];
            }
        }
    }
    
    private void LoadEquipmentData(SaveData saveData)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.playerCharacter == null) return;
        
        CharacterData character = CharacterManager.Instance.playerCharacter;
        
        Array slots = System.Enum.GetValues(typeof(EquipmentSlot));
        for (int i = 0; i < slots.Length && i < saveData.equippedItemIds.Count; i++)
        {
            EquipmentSlot slot = (EquipmentSlot)slots.GetValue(i);
            int itemId = saveData.equippedItemIds[i];
            
            if (itemId >= 0)
            {
                EquipmentConfig config = EquipmentManager.Instance?.GetEquipmentById(itemId);
                if (config != null)
                {
                    EquipmentInstance instance = EquipmentGenerator.Instance?.GenerateEquipment(itemId);
                    if (instance != null)
                    {
                        instance.enhanceLevel = saveData.equippedEnhanceLevels[i];
                        character.equippedItems[slot] = instance;
                    }
                }
            }
        }
        
        for (int i = 0; i < saveData.inventoryItemIds.Count; i++)
        {
            int itemId = saveData.inventoryItemIds[i];
            EquipmentConfig config = EquipmentManager.Instance?.GetEquipmentById(itemId);
            
            if (config != null)
            {
                EquipmentInstance instance = EquipmentGenerator.Instance?.GenerateEquipment(itemId);
                if (instance != null)
                {
                    instance.enhanceLevel = saveData.inventoryEnhanceLevels[i];
                    character.inventory.Add(instance);
                }
            }
        }
        
        character.CalculateTotalStats();
    }
    
    private void LoadQuestData(SaveData saveData)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.playerCharacter == null) return;
        
        CharacterData character = CharacterManager.Instance.playerCharacter;
        
        character.activeQuests.Clear();
        character.completedQuests.Clear();
        
        for (int i = 0; i < saveData.activeQuestIds.Count; i++)
        {
            QuestConfig config = QuestManager.Instance?.GetQuestById(saveData.activeQuestIds[i]);
            if (config != null)
            {
                QuestInstance instance = new QuestInstance
                {
                    config = config,
                    currentProgress = saveData.activeQuestProgress[i],
                    status = (QuestStatus)saveData.activeQuestStatus[i]
                };
                character.activeQuests.Add(instance);
            }
        }
        
        foreach (int questId in saveData.completedQuestIds)
        {
            QuestConfig config = QuestManager.Instance?.GetQuestById(questId);
            if (config != null)
            {
                QuestInstance instance = new QuestInstance
                {
                    config = config,
                    currentProgress = 0,
                    status = QuestStatus.TurnedIn
                };
                character.completedQuests.Add(instance);
            }
        }
    }
    
    private int GetClassId(ClassConfig config)
    {
        if (ClassManager.Instance != null)
        {
            return ClassManager.Instance.availableClasses.IndexOf(config);
        }
        return -1;
    }
    
    private ClassConfig GetClassById(int id)
    {
        if (ClassManager.Instance != null && id >= 0 && id < ClassManager.Instance.availableClasses.Count)
        {
            return ClassManager.Instance.availableClasses[id];
        }
        return null;
    }
}

[System.Serializable]
public class SaveData
{
    public string characterName;
    public int classId;
    public int level;
    public int experience;
    public int experienceToNextLevel;
    
    public int currentHealth;
    public int currentMana;
    public int gold;
    
    public CharacterStats baseStats;
    public CharacterStats totalStats;
    
    public List<int> skillLevels;
    
    public List<int> equippedItemIds;
    public List<int> equippedEnhanceLevels;
    public List<int> inventoryItemIds;
    public List<int> inventoryEnhanceLevels;
    
    public List<int> activeQuestIds;
    public List<int> activeQuestProgress;
    public List<int> activeQuestStatus;
    public List<int> completedQuestIds;
}
