using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    [Header("战斗设置")]
    public float globalDamageMultiplier = 1f;
    public float globalExperienceMultiplier = 1f;
    public float globalGoldMultiplier = 1f;
    
    [Header("战斗状态")]
    public bool inCombat = false;
    public List<MonsterController> activeMonsters;
    public PlayerController player;
    
    [Header("特效")]
    public GameObject defaultHitEffect;
    public GameObject defaultDeathEffect;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    private void Update()
    {
        CheckCombatState();
    }
    
    private void CheckCombatState()
    {
        activeMonsters.RemoveAll(m => m == null || m.isDead);
        
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerController>();
            }
        }
        
        bool wasInCombat = inCombat;
        inCombat = activeMonsters.Count > 0 && player != null && !player.isActiveAndEnabled == false;
        
        if (wasInCombat && !inCombat)
        {
            OnCombatEnd();
        }
        else if (!wasInCombat && inCombat)
        {
            OnCombatStart();
        }
    }
    
    private void OnCombatStart()
    {
        Debug.Log("战斗开始！");
    }
    
    private void OnCombatEnd()
    {
        Debug.Log("战斗结束！");
        
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.CheckDungeonCompletion();
        }
    }
    
    public void RegisterMonster(MonsterController monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
    }
    
    public void UnregisterMonster(MonsterController monster)
    {
        activeMonsters.Remove(monster);
    }
    
    public int CalculateDamage(CharacterStats attackerStats, CharacterStats defenderStats, bool isCritical = false)
    {
        int baseDamage = attackerStats.attackPower;
        int defense = defenderStats.defense;
        
        int strengthBonus = Mathf.FloorToInt(attackerStats.strength * 0.5f);
        int intelligenceBonus = Mathf.FloorToInt(attackerStats.intelligence * 0.3f);
        
        int totalDamage = baseDamage + strengthBonus + intelligenceBonus;
        
        int actualDamage = Mathf.Max(1, totalDamage - defense);
        
        if (isCritical)
        {
            actualDamage = Mathf.FloorToInt(actualDamage * attackerStats.criticalDamage);
        }
        
        actualDamage = Mathf.FloorToInt(actualDamage * globalDamageMultiplier);
        
        return actualDamage;
    }
    
    public bool RollCriticalHit(float criticalChance)
    {
        return Random.value < criticalChance;
    }
    
    public bool RollDodge(float dodgeChance)
    {
        return Random.value < dodgeChance;
    }
    
    public void ApplyDamage(GameObject target, int damage, bool isCritical = false)
    {
        MonsterController monster = target.GetComponent<MonsterController>();
        if (monster != null)
        {
            monster.TakeDamage(damage);
            return;
        }
        
        PlayerController playerController = target.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            return;
        }
    }
    
    public void ShowDamageNumber(GameObject target, int damage, bool isCritical = false)
    {
        DamageNumberUI damageNumber = target.GetComponentInChildren<DamageNumberUI>();
        if (damageNumber != null)
        {
            damageNumber.ShowDamage(damage, isCritical);
        }
    }
    
    public void SpawnHitEffect(Vector3 position)
    {
        if (defaultHitEffect != null)
        {
            Instantiate(defaultHitEffect, position, Quaternion.identity);
        }
    }
    
    public void SpawnDeathEffect(Vector3 position)
    {
        if (defaultDeathEffect != null)
        {
            Instantiate(defaultDeathEffect, position, Quaternion.identity);
        }
    }
    
    public int CalculateExperienceReward(MonsterConfig monster)
    {
        int baseExperience = monster.experienceReward;
        return Mathf.FloorToInt(baseExperience * globalExperienceMultiplier);
    }
    
    public int CalculateGoldReward(MonsterConfig monster)
    {
        int baseGold = monster.goldReward;
        return Mathf.FloorToInt(baseGold * globalGoldMultiplier);
    }
}
