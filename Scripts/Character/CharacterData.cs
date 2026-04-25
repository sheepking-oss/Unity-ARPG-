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
    [SerializeField]
    private CharacterStats _baseStats;
    [SerializeField]
    private CharacterStats _totalStats;
    [SerializeField]
    private CharacterStats _equipmentStatsCache;
    
    [Header("技能")]
    public List<SkillInstance> skills;
    public List<float> skillCooldowns;
    
    [Header("装备")]
    public Dictionary<EquipmentSlot, EquipmentInstance> equippedItems;
    public List<EquipmentInstance> inventory;
    
    [Header("任务")]
    public List<QuestInstance> activeQuests;
    public List<QuestInstance> completedQuests;
    
    [Header("装备状态标记")]
    private HashSet<int> _equippedItemIds = new HashSet<int>();
    private bool _isStatsDirty = true;
    
    public CharacterStats baseStats
    {
        get { return _baseStats; }
        set 
        { 
            _baseStats = value; 
            _isStatsDirty = true;
        }
    }
    
    public CharacterStats totalStats
    {
        get 
        {
            if (_isStatsDirty)
            {
                CalculateTotalStats();
            }
            return _totalStats; 
        }
        private set { _totalStats = value; }
    }
    
    public CharacterStats equipmentStatsCache
    {
        get { return _equipmentStatsCache; }
    }
    
    public void Initialize(ClassConfig config)
    {
        classConfig = config;
        characterName = config.className;
        level = 1;
        experience = 0;
        experienceToNextLevel = CalculateExperienceToNextLevel(level);
        
        _baseStats = config.baseStats.DeepCopy();
        _totalStats = new CharacterStats();
        _equipmentStatsCache = new CharacterStats();
        _equippedItemIds = new HashSet<int>();
        _isStatsDirty = true;
        
        currentHealth = _baseStats.maxHealth;
        currentMana = _baseStats.maxMana;
        
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
        
        Debug.Log($"[CharacterData] 初始化角色：{characterName}，基础属性：{_baseStats}");
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
        
        _baseStats += classConfig.perLevelStats;
        currentHealth = _baseStats.maxHealth;
        currentMana = _baseStats.maxMana;
        _isStatsDirty = true;
        
        CalculateTotalStats();
        
        Debug.Log($"[CharacterData] 等级提升！当前等级：{level}，新基础属性：{_baseStats}");
    }
    
    private int CalculateExperienceToNextLevel(int currentLevel)
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(1.5f, currentLevel - 1));
    }
    
    public void CalculateTotalStats()
    {
        if (_baseStats == null)
        {
            Debug.LogError("[CharacterData] baseStats 为空！");
            return;
        }
        
        _totalStats = _baseStats.DeepCopy();
        
        if (_equipmentStatsCache == null)
        {
            _equipmentStatsCache = new CharacterStats();
        }
        _equipmentStatsCache.Reset();
        
        if (_equippedItemIds == null)
        {
            _equippedItemIds = new HashSet<int>();
        }
        _equippedItemIds.Clear();
        
        int equipmentCount = 0;
        
        foreach (var kvp in equippedItems)
        {
            EquipmentSlot slot = kvp.Key;
            EquipmentInstance equippedItem = kvp.Value;
            
            if (equippedItem == null || equippedItem.config == null)
            {
                continue;
            }
            
            int itemId = equippedItem.config.equipmentId;
            
            if (_equippedItemIds.Contains(itemId))
            {
                Debug.LogWarning($"[CharacterData] 检测到重复装备！槽位 {slot} 中的物品 ID {itemId} 已存在，跳过计算");
                continue;
            }
            
            _equippedItemIds.Add(itemId);
            
            CharacterStats itemStats = equippedItem.GetTotalStats();
            _equipmentStatsCache += itemStats;
            _totalStats += itemStats;
            equipmentCount++;
            
            Debug.Log($"[CharacterData] 计算装备属性：槽位={slot}, 物品={equippedItem.config.equipmentName}, 属性加成={itemStats}");
        }
        
        _isStatsDirty = false;
        
        Debug.Log($"[CharacterData] 属性计算完成 - 装备数量：{equipmentCount}, 装备加成：{_equipmentStatsCache}, 总属性：{_totalStats}");
    }
    
    public bool EquipItem(EquipmentInstance item)
    {
        if (item == null || item.config == null)
        {
            Debug.LogError("[CharacterData] 尝试装备空物品！");
            return false;
        }
        
        if (!classConfig.allowedEquipmentTypes.Contains(item.config.equipmentType))
        {
            Debug.Log($"[CharacterData] 无法装备此类型的物品：{item.config.equipmentType}");
            return false;
        }
        
        if (item.config.requiredLevel > level)
        {
            Debug.Log($"[CharacterData] 等级不足，无法装备：需要 {item.config.requiredLevel} 级，当前 {level} 级");
            return false;
        }
        
        EquipmentSlot slot = item.config.slot;
        int itemId = item.config.equipmentId;
        
        if (_equippedItemIds.Contains(itemId))
        {
            Debug.LogWarning($"[CharacterData] 物品 {item.config.equipmentName} (ID:{itemId}) 已经装备在身上！");
            return false;
        }
        
        Debug.Log($"[CharacterData] 开始装备物品：{item.config.equipmentName} (ID:{itemId}) 到槽位：{slot}");
        
        EquipmentInstance oldItem = null;
        
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            oldItem = equippedItems[slot];
            int oldItemId = oldItem.config.equipmentId;
            
            Debug.Log($"[CharacterData] 槽位 {slot} 已有装备：{oldItem.config.equipmentName} (ID:{oldItemId})，将其移回背包");
            
            _equippedItemIds.Remove(oldItemId);
            inventory.Add(oldItem);
        }
        
        equippedItems[slot] = item;
        
        bool removedFromInventory = inventory.Remove(item);
        if (!removedFromInventory)
        {
            Debug.LogWarning($"[CharacterData] 物品 {item.config.equipmentName} 不在背包中，但仍然装备成功");
        }
        
        _equippedItemIds.Add(itemId);
        _isStatsDirty = true;
        
        CalculateTotalStats();
        
        Debug.Log($"[CharacterData] 装备完成：{item.config.equipmentName}，当前总属性：{_totalStats}");
        
        return true;
    }
    
    public void UnequipItem(EquipmentSlot slot)
    {
        if (!equippedItems.ContainsKey(slot) || equippedItems[slot] == null)
        {
            Debug.LogWarning($"[CharacterData] 槽位 {slot} 没有可卸下的装备");
            return;
        }
        
        EquipmentInstance itemToUnequip = equippedItems[slot];
        int itemId = itemToUnequip.config.equipmentId;
        
        Debug.Log($"[CharacterData] 开始卸下装备：槽位={slot}, 物品={itemToUnequip.config.equipmentName} (ID:{itemId})");
        
        if (!_equippedItemIds.Contains(itemId))
        {
            Debug.LogWarning($"[CharacterData] 物品 {itemToUnequip.config.equipmentName} 不在已装备列表中，但仍然从槽位卸下");
        }
        else
        {
            _equippedItemIds.Remove(itemId);
        }
        
        inventory.Add(itemToUnequip);
        equippedItems[slot] = null;
        _isStatsDirty = true;
        
        CalculateTotalStats();
        
        Debug.Log($"[CharacterData] 卸下装备完成：{itemToUnequip.config.equipmentName}，当前总属性：{_totalStats}");
    }
    
    public void ForceRecalculateStats()
    {
        Debug.Log("[CharacterData] 强制重新计算属性...");
        _isStatsDirty = true;
        CalculateTotalStats();
    }
    
    public void ValidateEquippedItems()
    {
        Debug.Log("[CharacterData] 开始验证装备状态...");
        
        HashSet<int> actualItemIds = new HashSet<int>();
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var kvp in equippedItems)
        {
            EquipmentSlot slot = kvp.Key;
            EquipmentInstance item = kvp.Value;
            
            if (item == null || item.config == null)
            {
                Debug.LogWarning($"[CharacterData] 槽位 {slot} 中的装备为空或配置无效");
                invalidCount++;
                continue;
            }
            
            int itemId = item.config.equipmentId;
            
            if (actualItemIds.Contains(itemId))
            {
                Debug.LogError($"[CharacterData] 检测到严重问题：物品 {item.config.equipmentName} (ID:{itemId}) 在多个槽位中存在！");
                invalidCount++;
            }
            else
            {
                actualItemIds.Add(itemId);
                validCount++;
            }
        }
        
        if (_equippedItemIds == null)
        {
            _equippedItemIds = new HashSet<int>();
        }
        
        bool idsMatch = _equippedItemIds.SetEquals(actualItemIds);
        
        if (!idsMatch)
        {
            Debug.LogWarning($"[CharacterData] 装备ID缓存与实际槽位不匹配！缓存数量：{_equippedItemIds.Count}, 实际数量：{actualItemIds.Count}");
            _equippedItemIds = actualItemIds;
            _isStatsDirty = true;
            CalculateTotalStats();
        }
        
        Debug.Log($"[CharacterData] 装备验证完成 - 有效装备：{validCount}, 无效装备：{invalidCount}, ID缓存匹配：{idsMatch}");
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
        Debug.Log("[CharacterData] 角色死亡！");
    }
    
    public string GetDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 角色属性调试信息 ===");
        sb.AppendLine($"角色：{characterName} (等级 {level})");
        sb.AppendLine($"基础属性：{_baseStats}");
        sb.AppendLine($"装备加成：{_equipmentStatsCache}");
        sb.AppendLine($"总属性：{_totalStats}");
        sb.AppendLine($"属性需要刷新：{_isStatsDirty}");
        sb.AppendLine($"\n已装备物品 (数量：{_equippedItemIds.Count})：");
        
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null && kvp.Value.config != null)
            {
                sb.AppendLine($"  槽位 {kvp.Key}: {kvp.Value.config.equipmentName} (ID:{kvp.Value.config.equipmentId})");
            }
        }
        
        sb.AppendLine($"\n装备ID缓存 (数量：{_equippedItemIds.Count})：");
        foreach (int id in _equippedItemIds)
        {
            sb.AppendLine($"  ID: {id}");
        }
        
        return sb.ToString();
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
    
    private CharacterStats _cachedStats;
    private bool _isCacheDirty = true;
    
    public CharacterStats GetTotalStats()
    {
        if (_isCacheDirty || _cachedStats == null)
        {
            CalculateStats();
        }
        return _cachedStats;
    }
    
    private void CalculateStats()
    {
        if (config == null)
        {
            _cachedStats = new CharacterStats();
            _isCacheDirty = false;
            return;
        }
        
        _cachedStats = config.baseStats.DeepCopy();
        
        float enhanceMultiplier = 1f + (config.enhanceStatBonusPerLevel * enhanceLevel);
        _cachedStats.maxHealth = Mathf.FloorToInt(_cachedStats.maxHealth * enhanceMultiplier);
        _cachedStats.maxMana = Mathf.FloorToInt(_cachedStats.maxMana * enhanceMultiplier);
        _cachedStats.attackPower = Mathf.FloorToInt(_cachedStats.attackPower * enhanceMultiplier);
        _cachedStats.defense = Mathf.FloorToInt(_cachedStats.defense * enhanceMultiplier);
        
        if (prefixes != null)
        {
            foreach (var prefix in prefixes)
            {
                if (prefix != null)
                {
                    _cachedStats += prefix.GetStats();
                }
            }
        }
        
        if (suffixes != null)
        {
            foreach (var suffix in suffixes)
            {
                if (suffix != null)
                {
                    _cachedStats += suffix.GetStats();
                }
            }
        }
        
        _isCacheDirty = false;
    }
    
    public void MarkDirty()
    {
        _isCacheDirty = true;
    }
}

[System.Serializable]
public class AffixInstance
{
    public AffixConfig config;
    public float value;
    
    public CharacterStats GetStats()
    {
        if (config == null)
        {
            return new CharacterStats();
        }
        
        float denominator = config.maxValue - config.minValue;
        float multiplier = denominator > 0 ? value / denominator : 0f;
        
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
