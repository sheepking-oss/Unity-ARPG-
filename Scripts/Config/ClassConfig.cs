using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Class", menuName = "ARPG/Class Config")]
public class ClassConfig : ScriptableObject
{
    [Header("基本信息")]
    public string className = "新职业";
    public Sprite classIcon;
    public string description = "职业描述";
    
    [Header("基础属性")]
    public CharacterStats baseStats;
    
    [Header("成长属性")]
    public CharacterStats perLevelStats;
    
    [Header("技能列表")]
    public List<SkillConfig> skills;
    
    [Header("装备类型")]
    public List<EquipmentType> allowedEquipmentTypes;
}

public enum EquipmentType
{
    Sword,
    Staff,
    Bow,
    Dagger,
    Shield,
    Helmet,
    Armor,
    Gloves,
    Boots,
    Ring,
    Amulet
}
