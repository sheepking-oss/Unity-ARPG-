using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterController : MonoBehaviour
{
    [Header("引用")]
    public CharacterController characterController;
    public Animator animator;
    public Transform target;
    
    [Header("怪物配置")]
    public MonsterConfig config;
    
    [Header("战斗状态")]
    public int currentHealth;
    public int currentLevel;
    public bool isDead = false;
    public bool isStunned = false;
    
    [Header("AI状态")]
    public MonsterAIState currentState = MonsterAIState.Idle;
    private Vector3 initialPosition;
    private float attackCooldownTimer = 0f;
    private float stunTimer = 0f;
    
    [Header("Boss阶段 (仅Boss)")]
    public int currentPhase = 0;
    private List<BossPhase> phases;
    
    [Header("特效")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public GameObject levelUpEffect;
    
    [Header("UI")]
    public MonsterHealthBar healthBar;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Update()
    {
        if (isDead) return;
        
        HandleStun();
        
        if (!isStunned)
        {
            UpdateAI();
        }
    }
    
    public void Initialize()
    {
        if (config == null) return;
        
        currentHealth = config.stats.maxHealth;
        currentLevel = config.level;
        initialPosition = transform.position;
        isDead = false;
        isStunned = false;
        
        if (config.isBoss && config.bossPhases != null && config.bossPhases.Count > 0)
        {
            phases = config.bossPhases;
            currentPhase = 0;
        }
        
        if (healthBar != null)
        {
            healthBar.Initialize(currentHealth, config.stats.maxHealth, config.monsterName, config.isBoss);
        }
        
        Debug.Log($"怪物初始化：{config.monsterName}，等级：{currentLevel}");
    }
    
    private void HandleStun()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                if (animator != null)
                {
                    animator.SetBool("IsStunned", false);
                }
            }
        }
    }
    
    private void UpdateAI()
    {
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
        
        FindTarget();
        
        switch (currentState)
        {
            case MonsterAIState.Idle:
                HandleIdle();
                break;
            case MonsterAIState.Patrol:
                HandlePatrol();
                break;
            case MonsterAIState.Chase:
                HandleChase();
                break;
            case MonsterAIState.Attack:
                HandleAttack();
                break;
            case MonsterAIState.Return:
                HandleReturn();
                break;
        }
    }
    
    private void FindTarget()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    private void HandleIdle()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
        
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget <= config.detectionRange)
            {
                currentState = MonsterAIState.Chase;
            }
        }
    }
    
    private void HandlePatrol()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0.5f);
        }
        
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget <= config.detectionRange)
            {
                currentState = MonsterAIState.Chase;
            }
        }
    }
    
    private void HandleChase()
    {
        if (target == null)
        {
            currentState = MonsterAIState.Return;
            return;
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float distanceToInitial = Vector3.Distance(transform.position, initialPosition);
        
        if (distanceToInitial > config.detectionRange * 2)
        {
            currentState = MonsterAIState.Return;
            return;
        }
        
        if (distanceToTarget <= config.attackRange)
        {
            currentState = MonsterAIState.Attack;
        }
        else
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                10f * Time.deltaTime
            );
            
            characterController.Move(direction * config.moveSpeed * Time.deltaTime);
            
            if (animator != null)
            {
                animator.SetFloat("Speed", config.moveSpeed);
            }
        }
    }
    
    private void HandleAttack()
    {
        if (target == null)
        {
            currentState = MonsterAIState.Return;
            return;
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget > config.attackRange * 1.5f)
        {
            currentState = MonsterAIState.Chase;
            return;
        }
        
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            10f * Time.deltaTime
        );
        
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
        
        if (attackCooldownTimer <= 0)
        {
            PerformAttack();
        }
    }
    
    private void PerformAttack()
    {
        float attackSpeedMultiplier = 1f;
        float damageMultiplier = 1f;
        
        if (config.isBoss && phases != null && currentPhase < phases.Count)
        {
            attackSpeedMultiplier = phases[currentPhase].phaseAttackSpeedMultiplier;
            damageMultiplier = phases[currentPhase].phaseDamageMultiplier;
        }
        
        attackCooldownTimer = config.attackCooldown / attackSpeedMultiplier;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        int damage = Mathf.FloorToInt(config.stats.attackPower * damageMultiplier);
        
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log($"{config.monsterName} 攻击玩家，造成 {damage} 点伤害");
        }
    }
    
    private void HandleReturn()
    {
        float distanceToInitial = Vector3.Distance(transform.position, initialPosition);
        
        if (distanceToInitial < 0.5f)
        {
            currentState = config.behavior == MonsterBehavior.Patrol ? MonsterAIState.Patrol : MonsterAIState.Idle;
            currentHealth = config.stats.maxHealth;
            
            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth, config.stats.maxHealth);
            }
            
            return;
        }
        
        Vector3 direction = (initialPosition - transform.position).normalized;
        direction.y = 0;
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            10f * Time.deltaTime
        );
        
        characterController.Move(direction * config.moveSpeed * Time.deltaTime);
        
        if (animator != null)
        {
            animator.SetFloat("Speed", config.moveSpeed);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        int actualDamage = Mathf.Max(1, damage - config.stats.defense);
        currentHealth -= actualDamage;
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);
        }
        
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, config.stats.maxHealth);
            healthBar.ShowDamageNumber(actualDamage);
        }
        
        if (currentState == MonsterAIState.Idle || currentState == MonsterAIState.Patrol)
        {
            currentState = MonsterAIState.Chase;
        }
        
        CheckBossPhase();
        
        if (currentHealth <= 0)
        {
            Die();
        }
        
        Debug.Log($"{config.monsterName} 受到 {actualDamage} 点伤害，剩余生命值：{currentHealth}");
    }
    
    private void CheckBossPhase()
    {
        if (!config.isBoss || phases == null || phases.Count == 0) return;
        
        float healthPercent = (float)currentHealth / config.stats.maxHealth;
        
        for (int i = phases.Count - 1; i >= 0; i--)
        {
            if (healthPercent <= phases[i].healthThreshold && i > currentPhase)
            {
                EnterPhase(i);
                break;
            }
        }
    }
    
    private void EnterPhase(int phaseIndex)
    {
        currentPhase = phaseIndex;
        BossPhase phase = phases[phaseIndex];
        
        Debug.Log($"{config.monsterName} 进入阶段：{phase.phaseName}");
        
        if (phase.phaseEffect != null)
        {
            Instantiate(phase.phaseEffect, transform.position, Quaternion.identity);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("PhaseChange");
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position + Vector3.up, Quaternion.identity);
        }
        
        if (healthBar != null)
        {
            healthBar.Hide();
        }
        
        DropLoot();
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.AddExperience(config.experienceReward);
            CharacterManager.Instance.AddGold(config.goldReward);
        }
        
        StartCoroutine(DestroyAfterDelay(2f));
        
        Debug.Log($"{config.monsterName} 死亡！");
    }
    
    private void DropLoot()
    {
        if (config.dropTable == null) return;
        
        List<DropResult> drops = config.dropTable.CalculateDrops();
        
        foreach (var drop in drops)
        {
            switch (drop.type)
            {
                case DropType.Gold:
                    if (CharacterManager.Instance != null)
                    {
                        CharacterManager.Instance.AddGold(drop.amount);
                    }
                    Debug.Log($"掉落金币：{drop.amount}");
                    break;
                    
                case DropType.Equipment:
                    EquipmentInstance equipment = EquipmentGenerator.Instance.GenerateEquipment(drop.itemId);
                    if (equipment != null && CharacterManager.Instance != null)
                    {
                        CharacterManager.Instance.AddToInventory(equipment);
                    }
                    Debug.Log($"掉落装备：{equipment?.config.equipmentName}");
                    break;
                    
                case DropType.Material:
                    Debug.Log($"掉落材料：ID={drop.itemId}，数量={drop.amount}");
                    break;
            }
        }
    }
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        
        if (animator != null)
        {
            animator.SetBool("IsStunned", true);
        }
    }
    
    public void ApplyKnockback(Vector3 direction, float force)
    {
        characterController.Move(direction * force);
    }
}

public enum MonsterAIState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Return
}
