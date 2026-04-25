using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    [Header("基本信息")]
    public string characterName;
    public ClassConfig classConfig;
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    
    [Header("资源")]
    public int currentHealth;
    public int currentMana;
    public int gold = 100;
    
    [Header("属性")]
    public CharacterStats baseStats;
    public CharacterStats totalStats;
    
    [Header("技能")]
    public List<SkillInstance> skills;
    public List<float> skillCooldowns;
    
    [Header("装备")]
    public Dictionary<EquipmentSlot, EquipmentInstance> equippedItems;
    public List<EquipmentInstance> inventory;
    
    [Header("任务")]
    public List<QuestInstance> activeQuests;
    public List<QuestInstance> completedQuests;
    
    public void Initialize(ClassConfig config)
    {
        classConfig = config;
        characterName = config.className;
        level = 1;
        experience = 0;
        experienceToNextLevel = CalculateExperienceToNextLevel(level);
        
        baseStats = config.baseStats;
        currentHealth = baseStats.maxHealth;
        currentMana = baseStats.maxMana;
        
        skills = new List<SkillInstance>();
        skillCooldowns = new List<float>();
        foreach (var skill in config.skills)
        {
            skills.Add(new SkillInstance { config = skill, level = 1 });
            skillCooldowns.Add(0f);
        }
        
        equippedItems = new Dictionary<EquipmentSlot, EquipmentInstance>();
        inventory = new List<EquipmentInstance>();
        
        activeQuests = new List<QuestInstance>();
        completedQuests = new List<QuestInstance>();
        
        CalculateTotalStats();
    }
    
    public void AddExperience(int amount)
    {
        experience += amount;
        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        experience -= experienceToNextLevel;
        level++;
        experienceToNextLevel = CalculateExperienceToNextLevel(level);
        
        baseStats += classConfig.perLevelStats;
        currentHealth = baseStats.maxHealth;
        currentMana = baseStats.maxMana;
        
        CalculateTotalStats();
        
        Debug.Log($"等级提升！当前等级：{level}");
    }
    
    private int CalculateExperienceToNextLevel(int currentLevel)
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(1.5f, currentLevel - 1));
    }
    
    public void CalculateTotalStats()
    {
        totalStats = baseStats;
        
        foreach (var equippedItem in equippedItems.Values)
        {
            if (equippedItem != null)
            {
                totalStats += equippedItem.GetTotalStats();
            }
        }
    }
    
    public bool EquipItem(EquipmentInstance item)
    {
        if (!classConfig.allowedEquipmentTypes.Contains(item.config.equipmentType))
        {
            Debug.Log("无法装备此类型的物品");
            return false;
        }
        
        if (item.config.requiredLevel > level)
        {
            Debug.Log("等级不足，无法装备");
            return false;
        }
        
        EquipmentSlot slot = item.config.slot;
        
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            inventory.Add(equippedItems[slot]);
        }
        
        equippedItems[slot] = item;
        inventory.Remove(item);
        
        CalculateTotalStats();
        
        Debug.Log($"装备了：{item.config.equipmentName}");
        return true;
    }
    
    public void UnequipItem(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            inventory.Add(equippedItems[slot]);
            equippedItems[slot] = null;
            CalculateTotalStats();
        }
    }
    
    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - totalStats.defense);
        currentHealth -= actualDamage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, totalStats.maxHealth);
    }
    
    public void UseMana(int amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
    }
    
    public void RestoreMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, totalStats.maxMana);
    }
    
    private void Die()
    {
        Debug.Log("角色死亡！");
    }
}

[System.Serializable]
public class SkillInstance
{
    public SkillConfig config;
    public int level;
    
    public float GetCooldown()
    {
        return config.cooldown;
    }
    
    public float GetManaCost()
    {
        return config.manaCost;
    }
    
    public float GetDamage()
    {
        return config.baseDamage + (config.damagePerLevel * (level - 1));
    }
}

[System.Serializable]
public class EquipmentInstance
{
    public EquipmentConfig config;
    public int enhanceLevel;
    public List<AffixInstance> prefixes;
    public List<AffixInstance> suffixes;
    
    public CharacterStats GetTotalStats()
    {
        CharacterStats stats = config.baseStats;
        
        float enhanceMultiplier = 1f + (config.enhanceStatBonusPerLevel * enhanceLevel);
        stats.maxHealth = Mathf.FloorToInt(stats.maxHealth * enhanceMultiplier);
        stats.maxMana = Mathf.FloorToInt(stats.maxMana * enhanceMultiplier);
        stats.attackPower = Mathf.FloorToInt(stats.attackPower * enhanceMultiplier);
        stats.defense = Mathf.FloorToInt(stats.defense * enhanceMultiplier);
        
        foreach (var prefix in prefixes)
        {
            stats += prefix.GetStats();
        }
        
        foreach (var suffix in suffixes)
        {
            stats += suffix.GetStats();
        }
        
        return stats;
    }
}

[System.Serializable]
public class AffixInstance
{
    public AffixConfig config;
    public float value;
    
    public CharacterStats GetStats()
    {
        float multiplier = value / (config.maxValue - config.minValue);
        CharacterStats stats = new CharacterStats();
        
        stats.maxHealth = Mathf.FloorToInt(config.stats.maxHealth * multiplier);
        stats.maxMana = Mathf.FloorToInt(config.stats.maxMana * multiplier);
        stats.strength = Mathf.FloorToInt(config.stats.strength * multiplier);
        stats.intelligence = Mathf.FloorToInt(config.stats.intelligence * multiplier);
        stats.agility = Mathf.FloorToInt(config.stats.agility * multiplier);
        stats.vitality = Mathf.FloorToInt(config.stats.vitality * multiplier);
        stats.attackPower = Mathf.FloorToInt(config.stats.attackPower * multiplier);
        stats.defense = Mathf.FloorToInt(config.stats.defense * multiplier);
        stats.attackSpeed = config.stats.attackSpeed * multiplier;
        stats.moveSpeed = config.stats.moveSpeed * multiplier;
        stats.criticalChance = config.stats.criticalChance * multiplier;
        stats.criticalDamage = config.stats.criticalDamage * multiplier;
        
        return stats;
    }
}

[System.Serializable]
public class QuestInstance
{
    public QuestConfig config;
    public QuestStatus status;
    public int currentProgress;
}

public enum QuestStatus
{
    NotStarted,
    InProgress,
    Completed,
    TurnedIn
}
