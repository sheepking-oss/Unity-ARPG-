using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    
    [Header("装备列表")]
    public List<EquipmentConfig> allEquipment;
    
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
        
        InitializeEquipmentDictionary();
    }
    
    private void InitializeEquipmentDictionary()
    {
        equipmentDictionary = new Dictionary<int, EquipmentConfig>();
        
        foreach (var equipment in allEquipment)
        {
            if (!equipmentDictionary.ContainsKey(equipment.equipmentId))
            {
                equipmentDictionary.Add(equipment.equipmentId, equipment);
            }
            else
            {
                Debug.LogWarning($"重复的装备ID：{equipment.equipmentId}");
            }
        }
    }
    
    public EquipmentConfig GetEquipmentById(int id)
    {
        if (equipmentDictionary.TryGetValue(id, out EquipmentConfig config))
        {
            return config;
        }
        return null;
    }
    
    public List<EquipmentConfig> GetEquipmentByType(EquipmentType type)
    {
        List<EquipmentConfig> result = new List<EquipmentConfig>();
        
        foreach (var equipment in allEquipment)
        {
            if (equipment.equipmentType == type)
            {
                result.Add(equipment);
            }
        }
        
        return result;
    }
    
    public List<EquipmentConfig> GetEquipmentBySlot(EquipmentSlot slot)
    {
        List<EquipmentConfig> result = new List<EquipmentConfig>();
        
        foreach (var equipment in allEquipment)
        {
            if (equipment.slot == slot)
            {
                result.Add(equipment);
            }
        }
        
        return result;
    }
    
    public List<EquipmentConfig> GetEquipmentByQuality(EquipmentQuality quality)
    {
        List<EquipmentConfig> result = new List<EquipmentConfig>();
        
        foreach (var equipment in allEquipment)
        {
            if (equipment.quality == quality)
            {
                result.Add(equipment);
            }
        }
        
        return result;
    }
    
    public EquipmentInstance CreateEquipmentInstance(EquipmentConfig config)
    {
        if (config == null) return null;
        
        EquipmentInstance instance = new EquipmentInstance
        {
            config = config,
            enhanceLevel = 0,
            prefixes = new List<AffixInstance>(),
            suffixes = new List<AffixInstance>()
        };
        
        GenerateRandomAffixes(instance);
        
        return instance;
    }
    
    private void GenerateRandomAffixes(EquipmentInstance instance)
    {
        EquipmentConfig config = instance.config;
        
        int prefixCount = 0;
        int suffixCount = 0;
        
        switch (config.quality)
        {
            case EquipmentQuality.Common:
                prefixCount = 0;
                suffixCount = 0;
                break;
            case EquipmentQuality.Uncommon:
                prefixCount = Random.Range(0, 2);
                suffixCount = Random.Range(0, 2);
                break;
            case EquipmentQuality.Rare:
                prefixCount = Random.Range(1, config.maxPrefixes + 1);
                suffixCount = Random.Range(1, config.maxSuffixes + 1);
                break;
            case EquipmentQuality.Epic:
                prefixCount = Random.Range(2, config.maxPrefixes + 1);
                suffixCount = Random.Range(2, config.maxSuffixes + 1);
                break;
            case EquipmentQuality.Legendary:
                prefixCount = config.maxPrefixes;
                suffixCount = config.maxSuffixes;
                break;
        }
        
        for (int i = 0; i < prefixCount; i++)
        {
            if (config.possiblePrefixes != null && config.possiblePrefixes.Count > 0)
            {
                AffixConfig prefix = config.possiblePrefixes[Random.Range(0, config.possiblePrefixes.Count)];
                AffixInstance prefixInstance = CreateAffixInstance(prefix);
                instance.prefixes.Add(prefixInstance);
            }
        }
        
        for (int i = 0; i < suffixCount; i++)
        {
            if (config.possibleSuffixes != null && config.possibleSuffixes.Count > 0)
            {
                AffixConfig suffix = config.possibleSuffixes[Random.Range(0, config.possibleSuffixes.Count)];
                AffixInstance suffixInstance = CreateAffixInstance(suffix);
                instance.suffixes.Add(suffixInstance);
            }
        }
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
    
    public string GetEquipmentFullName(EquipmentInstance instance)
    {
        if (instance == null || instance.config == null)
        {
            return "无";
        }
        
        string name = instance.config.equipmentName;
        
        if (instance.prefixes.Count > 0)
        {
            name = instance.prefixes[0].config.affixName + " " + name;
        }
        
        if (instance.suffixes.Count > 0)
        {
            name = name + " " + instance.suffixes[0].config.affixName;
        }
        
        if (instance.enhanceLevel > 0)
        {
            name = name + " +" + instance.enhanceLevel;
        }
        
        return name;
    }
    
    public Color GetQualityColor(EquipmentQuality quality)
    {
        switch (quality)
        {
            case EquipmentQuality.Common:
                return Color.gray;
            case EquipmentQuality.Uncommon:
                return Color.green;
            case EquipmentQuality.Rare:
                return Color.blue;
            case EquipmentQuality.Epic:
                return new Color(0.7f, 0.2f, 0.9f);
            case EquipmentQuality.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}
