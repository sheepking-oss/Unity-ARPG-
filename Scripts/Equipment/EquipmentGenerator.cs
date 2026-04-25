using UnityEngine;
using System.Collections.Generic;

public class EquipmentGenerator : MonoBehaviour
{
    public static EquipmentGenerator Instance { get; private set; }
    
    [Header("装备配置引用")]
    public List<EquipmentConfig> equipmentConfigs;
    public List<AffixConfig> allAffixes;
    
    [Header("生成概率")]
    [Range(0f, 1f)]
    public float commonChance = 0.5f;
    [Range(0f, 1f)]
    public float uncommonChance = 0.3f;
    [Range(0f, 1f)]
    public float rareChance = 0.15f;
    [Range(0f, 1f)]
    public float epicChance = 0.04f;
    [Range(0f, 1f)]
    public float legendaryChance = 0.01f;
    
    private Dictionary<int, EquipmentConfig> equipmentDictionary;
    
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
        
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        equipmentDictionary = new Dictionary<int, EquipmentConfig>();
        
        foreach (var config in equipmentConfigs)
        {
            if (!equipmentDictionary.ContainsKey(config.equipmentId))
            {
                equipmentDictionary.Add(config.equipmentId, config);
            }
        }
    }
    
    public EquipmentInstance GenerateEquipment(int equipmentId)
    {
        if (equipmentDictionary.TryGetValue(equipmentId, out EquipmentConfig config))
        {
            return GenerateEquipmentInstance(config);
        }
        
        Debug.LogWarning($"未找到装备ID：{equipmentId}");
        return null;
    }
    
    public EquipmentInstance GenerateRandomEquipment(int minLevel = 1, int maxLevel = 10)
    {
        List<EquipmentConfig> validEquipment = new List<EquipmentConfig>();
        
        foreach (var config in equipmentConfigs)
        {
            if (config.requiredLevel >= minLevel && config.requiredLevel <= maxLevel)
            {
                validEquipment.Add(config);
            }
        }
        
        if (validEquipment.Count == 0)
        {
            Debug.LogWarning("没有找到符合等级要求的装备");
            return null;
        }
        
        EquipmentConfig selectedConfig = validEquipment[Random.Range(0, validEquipment.Count)];
        return GenerateEquipmentInstance(selectedConfig);
    }
    
    public EquipmentInstance GenerateEquipmentByQuality(EquipmentQuality quality, int minLevel = 1, int maxLevel = 10)
    {
        List<EquipmentConfig> validEquipment = new List<EquipmentConfig>();
        
        foreach (var config in equipmentConfigs)
        {
            if (config.requiredLevel >= minLevel && config.requiredLevel <= maxLevel && config.quality == quality)
            {
                validEquipment.Add(config);
            }
        }
        
        if (validEquipment.Count == 0)
        {
            Debug.LogWarning($"没有找到符合要求的{quality}装备");
            return null;
        }
        
        EquipmentConfig selectedConfig = validEquipment[Random.Range(0, validEquipment.Count)];
        return GenerateEquipmentInstance(selectedConfig);
    }
    
    public EquipmentInstance GenerateEquipmentWithMagicFind(int minLevel = 1, int maxLevel = 10, float magicFind = 0f)
    {
        EquipmentQuality quality = RollQuality(magicFind);
        return GenerateEquipmentByQuality(quality, minLevel, maxLevel);
    }
    
    private EquipmentQuality RollQuality(float magicFindBonus = 0f)
    {
        float roll = Random.value;
        
        float legendaryThreshold = legendaryChance * (1 + magicFindBonus);
        float epicThreshold = legendaryThreshold + epicChance * (1 + magicFindBonus * 0.5f);
        float rareThreshold = epicThreshold + rareChance * (1 + magicFindBonus * 0.3f);
        float uncommonThreshold = rareThreshold + uncommonChance;
        
        if (roll < legendaryThreshold)
        {
            return EquipmentQuality.Legendary;
        }
        else if (roll < epicThreshold)
        {
            return EquipmentQuality.Epic;
        }
        else if (roll < rareThreshold)
        {
            return EquipmentQuality.Rare;
        }
        else if (roll < uncommonThreshold)
        {
            return EquipmentQuality.Uncommon;
        }
        else
        {
            return EquipmentQuality.Common;
        }
    }
    
    private EquipmentInstance GenerateEquipmentInstance(EquipmentConfig config)
    {
        if (config == null) return null;
        
        EquipmentInstance instance = new EquipmentInstance
        {
            config = config,
            enhanceLevel = 0,
            prefixes = new List<AffixInstance>(),
            suffixes = new List<AffixInstance>()
        };
        
        GenerateAffixes(instance);
        
        return instance;
    }
    
    private void GenerateAffixes(EquipmentInstance instance)
    {
        EquipmentConfig config = instance.config;
        
        int prefixCount = GetAffixCountByQuality(config.quality, true);
        int suffixCount = GetAffixCountByQuality(config.quality, false);
        
        for (int i = 0; i < prefixCount; i++)
        {
            AffixConfig prefix = GetRandomAffix(AffixType.Prefix, config.quality);
            if (prefix != null)
            {
                AffixInstance prefixInstance = CreateAffixInstance(prefix);
                instance.prefixes.Add(prefixInstance);
            }
        }
        
        for (int i = 0; i < suffixCount; i++)
        {
            AffixConfig suffix = GetRandomAffix(AffixType.Suffix, config.quality);
            if (suffix != null)
            {
                AffixInstance suffixInstance = CreateAffixInstance(suffix);
                instance.suffixes.Add(suffixInstance);
            }
        }
    }
    
    private int GetAffixCountByQuality(EquipmentQuality quality, bool isPrefix)
    {
        switch (quality)
        {
            case EquipmentQuality.Common:
                return 0;
            case EquipmentQuality.Uncommon:
                return isPrefix ? Random.Range(0, 2) : Random.Range(0, 2);
            case EquipmentQuality.Rare:
                return Random.Range(1, 4);
            case EquipmentQuality.Epic:
                return Random.Range(2, 5);
            case EquipmentQuality.Legendary:
                return 3;
            default:
                return 0;
        }
    }
    
    private AffixConfig GetRandomAffix(AffixType type, EquipmentQuality minimumQuality)
    {
        List<AffixConfig> validAffixes = new List<AffixConfig>();
        
        foreach (var affix in allAffixes)
        {
            if (affix.type == type && affix.requiredQuality <= minimumQuality)
            {
                validAffixes.Add(affix);
            }
        }
        
        if (validAffixes.Count == 0)
        {
            return null;
        }
        
        return validAffixes[Random.Range(0, validAffixes.Count)];
    }
    
    private AffixInstance CreateAffixInstance(AffixConfig config)
    {
        float value = Random.Range(config.minValue, config.maxValue);
        
        return new AffixInstance
        {
            config = config,
            value = value
        };
    }
}
