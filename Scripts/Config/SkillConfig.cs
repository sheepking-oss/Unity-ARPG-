using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill", menuName = "ARPG/Skill Config")]
public class SkillConfig : ScriptableObject
{
    [Header("基本信息")]
    public string skillName = "新技能";
    public Sprite skillIcon;
    public string description = "技能描述";
    public int skillId;
    
    [Header("技能属性")]
    public SkillType skillType;
    public TargetType targetType;
    public float cooldown = 5f;
    public float manaCost = 10f;
    public float castTime = 0f;
    public float range = 5f;
    
    [Header("伤害/治疗")]
    public float baseDamage = 20f;
    public float damagePerLevel = 5f;
    public float healAmount = 0f;
    
    [Header("特效")]
    public GameObject castEffect;
    public GameObject hitEffect;
    public AudioClip castSound;
    
    [Header("技能修饰")]
    public List<SkillModifier> modifiers;
}

public enum SkillType
{
    Attack,
    Heal,
    Buff,
    Debuff,
    Utility
}

public enum TargetType
{
    Self,
    SingleTarget,
    AreaOfEffect,
    Directional
}

[System.Serializable]
public class SkillModifier
{
    public ModifierType type;
    public float value;
    public float duration;
}

public enum ModifierType
{
    IncreaseAttackSpeed,
    IncreaseMoveSpeed,
    IncreaseDefense,
    DecreaseAttackSpeed,
    DecreaseMoveSpeed,
    Stun,
    Knockback,
    Bleed,
    Burn,
    Freeze
}
