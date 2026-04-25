using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("引用")]
    public CharacterController characterController;
    public Animator animator;
    public Transform cameraTransform;
    
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    
    [Header("闪避设置")]
    public float dodgeDistance = 5f;
    public float dodgeDuration = 0.3f;
    public float dodgeCooldown = 1f;
    public KeyCode dodgeKey = KeyCode.Space;
    public const int DODGE_SKILL_ID = -1;
    
    [Header("战斗设置")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public KeyCode attackKey = KeyCode.Mouse0;
    public const int ATTACK_SKILL_ID = -2;
    
    [Header("技能设置")]
    public KeyCode[] skillKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    
    private CharacterData characterData;
    private Vector3 velocity;
    private bool isDodging = false;
    private int playerId = 0;
    private bool isInitialized = false;
    
    private void OnEnable()
    {
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnCooldownsUpdated += OnCooldownsUpdated;
            TimerManager.Instance.OnBuffsUpdated += OnBuffsUpdated;
            TimerManager.Instance.OnSceneChanged += OnSceneChanged;
        }
    }
    
    private void OnDisable()
    {
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnCooldownsUpdated -= OnCooldownsUpdated;
            TimerManager.Instance.OnBuffsUpdated -= OnBuffsUpdated;
            TimerManager.Instance.OnSceneChanged -= OnSceneChanged;
        }
    }
    
    private void Update()
    {
        if (characterData == null || !isInitialized) return;
        
        if (!isDodging)
        {
            HandleMovement();
            HandleDodge();
            HandleAttack();
            HandleSkills();
        }
        
        ApplyGravity();
        ApplyBuffEffects();
    }
    
    public void Initialize(CharacterData data)
    {
        characterData = data;
        isInitialized = true;
        
        moveSpeed = characterData.totalStats.moveSpeed;
        
        SyncCooldownsFromCharacterData();
        
        Debug.Log($"[PlayerController] 初始化完成，已同步 {data.skills.Count} 个技能的冷却数据");
    }
    
    private void SyncCooldownsFromCharacterData()
    {
        if (TimerManager.Instance == null) return;
        if (characterData == null || characterData.skillCooldowns == null) return;
        
        for (int i = 0; i < characterData.skills.Count; i++)
        {
            if (i < characterData.skillCooldowns.Count)
            {
                float remainingCooldown = characterData.skillCooldowns[i];
                if (remainingCooldown > 0)
                {
                    SkillInstance skill = characterData.skills[i];
                    int skillId = skill.config.skillId;
                    
                    float existingCooldown = TimerManager.Instance.GetCooldownRemaining(skillId, playerId);
                    
                    if (existingCooldown <= 0)
                    {
                        TimerManager.Instance.StartCooldown(skillId, skill.config.skillName, remainingCooldown, playerId);
                        Debug.Log($"[PlayerController] 从存档恢复冷却: {skill.config.skillName}, 剩余: {remainingCooldown:F2}s");
                    }
                }
            }
        }
        
        SyncCharacterDataCooldowns();
    }
    
    private void SyncCharacterDataCooldowns()
    {
        if (TimerManager.Instance == null) return;
        if (characterData == null || characterData.skills == null) return;
        
        while (characterData.skillCooldowns.Count < characterData.skills.Count)
        {
            characterData.skillCooldowns.Add(0f);
        }
        
        for (int i = 0; i < characterData.skills.Count; i++)
        {
            if (i < characterData.skillCooldowns.Count)
            {
                SkillInstance skill = characterData.skills[i];
                float remaining = TimerManager.Instance.GetCooldownRemaining(skill.config.skillId, playerId);
                characterData.skillCooldowns[i] = remaining;
            }
        }
    }
    
    #region 冷却事件处理
    
    private void OnCooldownsUpdated()
    {
        SyncCharacterDataCooldowns();
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.UpdateAllUI();
        }
    }
    
    private void OnBuffsUpdated()
    {
        RecalculateStatsWithBuffs();
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.UpdateAllUI();
        }
    }
    
    private void OnSceneChanged()
    {
        Debug.Log("[PlayerController] 场景切换完成，验证冷却状态...");
        
        if (TimerManager.Instance != null)
        {
            var cooldowns = TimerManager.Instance.GetAllCooldowns(playerId);
            var buffs = TimerManager.Instance.GetAllBuffs(playerId);
            
            Debug.Log($"[PlayerController] 当前冷却数量: {cooldowns.Count}, Buff数量: {buffs.Count}");
        }
        
        SyncCharacterDataCooldowns();
    }
    
    #endregion
    
    #region 闪避系统
    
    private void HandleDodge()
    {
        if (Input.GetKeyDown(dodgeKey))
        {
            if (TimerManager.Instance != null)
            {
                if (TimerManager.Instance.IsCooldownReady(DODGE_SKILL_ID, playerId))
                {
                    StartCoroutine(Dodge());
                }
                else
                {
                    float remaining = TimerManager.Instance.GetCooldownRemaining(DODGE_SKILL_ID, playerId);
                    Debug.Log($"[PlayerController] 闪避冷却中: {remaining:F2}s");
                }
            }
        }
    }
    
    private IEnumerator Dodge()
    {
        isDodging = true;
        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartCooldown(DODGE_SKILL_ID, "闪避", dodgeCooldown, playerId);
        }
        
        Vector3 dodgeDirection = transform.forward;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (horizontal != 0 || vertical != 0)
        {
            if (cameraTransform != null)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();
                
                dodgeDirection = vertical * cameraForward + horizontal * cameraRight;
                dodgeDirection.Normalize();
            }
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Dodge");
        }
        
        float elapsedTime = 0f;
        while (elapsedTime < dodgeDuration)
        {
            characterController.Move(dodgeDirection * (dodgeDistance / dodgeDuration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isDodging = false;
    }
    
    #endregion
    
    #region 攻击系统
    
    private void HandleAttack()
    {
        if (Input.GetKeyDown(attackKey))
        {
            if (TimerManager.Instance != null)
            {
                if (TimerManager.Instance.IsCooldownReady(ATTACK_SKILL_ID, playerId))
                {
                    PerformAttack();
                }
                else
                {
                    float remaining = TimerManager.Instance.GetCooldownRemaining(ATTACK_SKILL_ID, playerId);
                    Debug.Log($"[PlayerController] 攻击冷却中: {remaining:F2}s");
                }
            }
        }
    }
    
    private void PerformAttack()
    {
        float actualCooldown = attackCooldown / characterData.totalStats.attackSpeed;
        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartCooldown(ATTACK_SKILL_ID, "普通攻击", actualCooldown, playerId);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, attackRange))
        {
            MonsterController monster = hit.collider.GetComponent<MonsterController>();
            if (monster != null)
            {
                int damage = CalculateDamage();
                bool isCritical = Random.value < characterData.totalStats.criticalChance;
                
                if (isCritical)
                {
                    damage = Mathf.FloorToInt(damage * characterData.totalStats.criticalDamage);
                    Debug.Log($"[PlayerController] 暴击！造成 {damage} 点伤害");
                }
                else
                {
                    Debug.Log($"[PlayerController] 造成 {damage} 点伤害");
                }
                
                monster.TakeDamage(damage);
            }
        }
    }
    
    private int CalculateDamage()
    {
        int baseDamage = characterData.totalStats.attackPower;
        int strengthBonus = Mathf.FloorToInt(characterData.totalStats.strength * 0.5f);
        
        return baseDamage + strengthBonus;
    }
    
    #endregion
    
    #region 技能系统
    
    private void HandleSkills()
    {
        for (int i = 0; i < skillKeys.Length; i++)
        {
            if (i >= characterData.skills.Count) break;
            
            if (Input.GetKeyDown(skillKeys[i]))
            {
                UseSkill(i);
            }
        }
    }
    
    private void UseSkill(int index)
    {
        if (index >= characterData.skills.Count) return;
        
        SkillInstance skill = characterData.skills[index];
        int skillId = skill.config.skillId;
        
        if (TimerManager.Instance != null)
        {
            if (!TimerManager.Instance.IsCooldownReady(skillId, playerId))
            {
                float remaining = TimerManager.Instance.GetCooldownRemaining(skillId, playerId);
                Debug.Log($"[PlayerController] 技能冷却中: {skill.config.skillName}, 剩余: {remaining:F2}s");
                return;
            }
        }
        
        if (characterData.currentMana < skill.GetManaCost())
        {
            Debug.Log("[PlayerController] 魔力不足！");
            return;
        }
        
        characterData.UseMana(Mathf.FloorToInt(skill.GetManaCost()));
        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartCooldown(skillId, skill.config.skillName, skill.GetCooldown(), playerId);
            Debug.Log($"[PlayerController] 使用技能: {skill.config.skillName}, 冷却: {skill.GetCooldown()}s");
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Skill");
        }
        
        ExecuteSkill(skill);
    }
    
    private void ExecuteSkill(SkillInstance skill)
    {
        switch (skill.config.skillType)
        {
            case SkillType.Attack:
                ExecuteAttackSkill(skill);
                break;
            case SkillType.Heal:
                ExecuteHealSkill(skill);
                break;
            case SkillType.Buff:
                ExecuteBuffSkill(skill);
                break;
            case SkillType.Debuff:
                ExecuteDebuffSkill(skill);
                break;
            case SkillType.Utility:
                ExecuteUtilitySkill(skill);
                break;
        }
    }
    
    private void ExecuteAttackSkill(SkillInstance skill)
    {
        float damage = skill.GetDamage();
        int intelligenceBonus = Mathf.FloorToInt(characterData.totalStats.intelligence * 0.5f);
        damage += intelligenceBonus;
        
        if (skill.config.targetType == TargetType.SingleTarget)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, skill.config.range))
            {
                MonsterController monster = hit.collider.GetComponent<MonsterController>();
                if (monster != null)
                {
                    monster.TakeDamage(Mathf.FloorToInt(damage));
                    Debug.Log($"[PlayerController] 技能造成 {Mathf.FloorToInt(damage)} 点伤害");
                }
            }
        }
        else if (skill.config.targetType == TargetType.AreaOfEffect)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, skill.config.range);
            foreach (var collider in hitColliders)
            {
                MonsterController monster = collider.GetComponent<MonsterController>();
                if (monster != null)
                {
                    monster.TakeDamage(Mathf.FloorToInt(damage));
                }
            }
            Debug.Log($"[PlayerController] AOE技能造成 {Mathf.FloorToInt(damage)} 点伤害");
        }
    }
    
    private void ExecuteHealSkill(SkillInstance skill)
    {
        characterData.Heal(Mathf.FloorToInt(skill.config.healAmount));
        Debug.Log($"[PlayerController] 恢复了 {Mathf.FloorToInt(skill.config.healAmount)} 点生命值");
    }
    
    private void ExecuteBuffSkill(SkillInstance skill)
    {
        if (TimerManager.Instance == null || skill.config.modifiers == null) return;
        
        foreach (var modifier in skill.config.modifiers)
        {
            int buffId = (int)modifier.type;
            string buffName = $"{skill.config.skillName} - {modifier.type}";
            
            int stacks = TimerManager.Instance.ApplyBuff(
                buffId,
                buffName,
                modifier.type,
                modifier.duration,
                modifier.value,
                1,
                playerId
            );
            
            Debug.Log($"[PlayerController] 应用Buff: {buffName}, 类型:{modifier.type}, 持续:{modifier.duration}s, 值:{modifier.value}, 层数:{stacks}");
        }
    }
    
    private void ExecuteDebuffSkill(SkillInstance skill)
    {
        Debug.Log($"[PlayerController] 使用了减益技能：{skill.config.skillName}");
    }
    
    private void ExecuteUtilitySkill(SkillInstance skill)
    {
        Debug.Log($"[PlayerController] 使用了功能性技能：{skill.config.skillName}");
    }
    
    #endregion
    
    #region Buff效果应用
    
    private void ApplyBuffEffects()
    {
        if (TimerManager.Instance == null) return;
        
        float attackSpeedBuff = TimerManager.Instance.GetBuffValue(ModifierType.IncreaseAttackSpeed, playerId);
        float moveSpeedBuff = TimerManager.Instance.GetBuffValue(ModifierType.IncreaseMoveSpeed, playerId);
        float defenseBuff = TimerManager.Instance.GetBuffValue(ModifierType.IncreaseDefense, playerId);
        
        if (moveSpeedBuff > 0)
        {
            float baseSpeed = characterData.totalStats.moveSpeed;
            moveSpeed = baseSpeed * (1 + moveSpeedBuff);
        }
        else
        {
            moveSpeed = characterData.totalStats.moveSpeed;
        }
    }
    
    private void RecalculateStatsWithBuffs()
    {
        if (characterData != null)
        {
            characterData.CalculateTotalStats();
        }
    }
    
    #endregion
    
    #region 移动和重力
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        
        if (movement.magnitude >= 0.1f)
        {
            if (cameraTransform != null)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();
                
                movement = vertical * cameraForward + horizontal * cameraRight;
            }
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(movement),
                rotationSpeed * Time.deltaTime
            );
            
            characterController.Move(movement.normalized * moveSpeed * Time.deltaTime);
            
            if (animator != null)
            {
                animator.SetFloat("Speed", movement.magnitude);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
        }
    }
    
    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    #endregion
    
    public void TakeDamage(int damage)
    {
        if (isDodging) return;
        
        if (TimerManager.Instance != null)
        {
            float defenseBuff = TimerManager.Instance.GetBuffValue(ModifierType.IncreaseDefense, playerId);
            float stunCheck = TimerManager.Instance.GetBuffValue(ModifierType.Stun, playerId);
            
            if (stunCheck > 0)
            {
                Debug.Log("[PlayerController] 眩晕免疫生效！");
                return;
            }
            
            int totalDefense = Mathf.FloorToInt(characterData.totalStats.defense * (1 + defenseBuff));
            int actualDamage = Mathf.Max(1, damage - totalDefense);
            
            characterData.TakeDamage(actualDamage);
        }
        else
        {
            characterData.TakeDamage(damage);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        Debug.Log($"[PlayerController] 受到伤害");
    }
    
    public string GetDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== PlayerController 调试信息 ===");
        sb.AppendLine($"初始化完成: {isInitialized}");
        sb.AppendLine($"玩家ID: {playerId}");
        sb.AppendLine($"闪避中: {isDodging}");
        
        if (TimerManager.Instance != null)
        {
            sb.AppendLine($"\n冷却状态:");
            var cooldowns = TimerManager.Instance.GetAllCooldowns(playerId);
            foreach (var cd in cooldowns)
            {
                sb.AppendLine($"  [{cd.skillId}] {cd.skillName}: {cd.remainingCooldown:F2}s / {cd.totalCooldown:F2}s");
            }
            
            sb.AppendLine($"\nBuff状态:");
            var buffs = TimerManager.Instance.GetAllBuffs(playerId);
            foreach (var buff in buffs)
            {
                sb.AppendLine($"  [{buff.buffId}] {buff.buffName} ({buff.buffType}): {buff.remainingDuration:F2}s, 层数:{buff.stackCount}, 值:{buff.value}");
            }
        }
        
        return sb.ToString();
    }
}
