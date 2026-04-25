using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Equipment", menuName = "ARPG/Equipment Config")]
public class EquipmentConfig : ScriptableObject
{
    [Header("基本信息")]
    public string equipmentName = "新装备";
    public Sprite equipmentIcon;
    public string description = "装备描述";
    public int equipmentId;
    
    [Header("装备属性")]
    public EquipmentType equipmentType;
    public EquipmentSlot slot;
    public EquipmentQuality quality;
    public int requiredLevel = 1;
    
    [Header("基础属性")]
    public CharacterStats baseStats;
    
    [Header("随机词条")]
    public List<AffixConfig> possiblePrefixes;
    public List<AffixConfig> possibleSuffixes;
    public int maxPrefixes = 3;
    public int maxSuffixes = 3;
    
    [Header("强化")]
    public int maxEnhanceLevel = 20;
    public float enhanceStatBonusPerLevel = 0.05f;
    public int enhanceCostPerLevel = 100;
    
    [Header("售卖")]
    public int buyPrice = 100;
    public int sellPrice = 50;
}

public enum EquipmentSlot
{
    Head,
    Chest,
    Gloves,
    Boots,
    MainHand,
    OffHand,
    Ring1,
    Ring2,
    Amulet
}

public enum EquipmentQuality
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "New Affix", menuName = "ARPG/Affix Config")]
public class AffixConfig : ScriptableObject
{
    public string affixName;
    public AffixType type;
    public EquipmentQuality requiredQuality;
    public CharacterStats stats;
    public float minValue;
    public float maxValue;
}

public enum AffixType
{
    Prefix,
    Suffix
}
