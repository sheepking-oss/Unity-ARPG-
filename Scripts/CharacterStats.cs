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
    
    public CharacterStats DeepCopy()
    {
        return new CharacterStats
        {
            maxHealth = this.maxHealth,
            maxMana = this.maxMana,
            strength = this.strength,
            intelligence = this.intelligence,
            agility = this.agility,
            vitality = this.vitality,
            attackPower = this.attackPower,
            defense = this.defense,
            attackSpeed = this.attackSpeed,
            moveSpeed = this.moveSpeed,
            criticalChance = this.criticalChance,
            criticalDamage = this.criticalDamage
        };
    }
    
    public void CopyFrom(CharacterStats other)
    {
        if (other == null) return;
        
        this.maxHealth = other.maxHealth;
        this.maxMana = other.maxMana;
        this.strength = other.strength;
        this.intelligence = other.intelligence;
        this.agility = other.agility;
        this.vitality = other.vitality;
        this.attackPower = other.attackPower;
        this.defense = other.defense;
        this.attackSpeed = other.attackSpeed;
        this.moveSpeed = other.moveSpeed;
        this.criticalChance = other.criticalChance;
        this.criticalDamage = other.criticalDamage;
    }
    
    public void Reset()
    {
        this.maxHealth = 0;
        this.maxMana = 0;
        this.strength = 0;
        this.intelligence = 0;
        this.agility = 0;
        this.vitality = 0;
        this.attackPower = 0;
        this.defense = 0;
        this.attackSpeed = 0f;
        this.moveSpeed = 0f;
        this.criticalChance = 0f;
        this.criticalDamage = 0f;
    }
    
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
    
    public override string ToString()
    {
        return string.Format(
            "HP:{0} MP:{1} STR:{2} INT:{3} AGI:{4} VIT:{5} ATK:{6} DEF:{7}",
            maxHealth, maxMana, strength, intelligence, agility, vitality, attackPower, defense
        );
    }
}
