using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    [Header("基础属性")]
    public int maxHealth = 100;
    public int maxMana = 50;
    public int strength = 10;
    public int intelligence = 10;
    public int agility = 10;
    public int vitality = 10;
    
    [Header("战斗属性")]
    public int attackPower = 10;
    public int defense = 5;
    public float attackSpeed = 1.0f;
    public float moveSpeed = 5.0f;
    public float criticalChance = 0.05f;
    public float criticalDamage = 1.5f;
    
    public static CharacterStats operator +(CharacterStats a, CharacterStats b)
    {
        return new CharacterStats
        {
            maxHealth = a.maxHealth + b.maxHealth,
            maxMana = a.maxMana + b.maxMana,
            strength = a.strength + b.strength,
            intelligence = a.intelligence + b.intelligence,
            agility = a.agility + b.agility,
            vitality = a.vitality + b.vitality,
            attackPower = a.attackPower + b.attackPower,
            defense = a.defense + b.defense,
            attackSpeed = a.attackSpeed + b.attackSpeed,
            moveSpeed = a.moveSpeed + b.moveSpeed,
            criticalChance = a.criticalChance + b.criticalChance,
            criticalDamage = a.criticalDamage + b.criticalDamage
        };
    }
    
    public static CharacterStats operator -(CharacterStats a, CharacterStats b)
    {
        return new CharacterStats
        {
            maxHealth = a.maxHealth - b.maxHealth,
            maxMana = a.maxMana - b.maxMana,
            strength = a.strength - b.strength,
            intelligence = a.intelligence - b.intelligence,
            agility = a.agility - b.agility,
            vitality = a.vitality - b.vitality,
            attackPower = a.attackPower - b.attackPower,
            defense = a.defense - b.defense,
            attackSpeed = a.attackSpeed - b.attackSpeed,
            moveSpeed = a.moveSpeed - b.moveSpeed,
            criticalChance = a.criticalChance - b.criticalChance,
            criticalDamage = a.criticalDamage - b.criticalDamage
        };
    }
}
