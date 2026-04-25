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
    
    [Header("战斗设置")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public KeyCode attackKey = KeyCode.Mouse0;
    
    [Header("技能设置")]
    public KeyCode[] skillKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    
    private CharacterData characterData;
    private Vector3 velocity;
    private bool isDodging = false;
    private float dodgeCooldownTimer = 0f;
    private float attackCooldownTimer = 0f;
    private float[] skillCooldownTimers;
    
    private void Update()
    {
        if (characterData == null) return;
        
        HandleCooldowns();
        
        if (!isDodging)
        {
            HandleMovement();
            HandleDodge();
            HandleAttack();
            HandleSkills();
        }
        
        ApplyGravity();
    }
    
    public void Initialize(CharacterData data)
    {
        characterData = data;
        skillCooldownTimers = new float[data.skills.Count];
        
        moveSpeed = characterData.totalStats.moveSpeed;
    }
    
    private void HandleCooldowns()
    {
        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }
        
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
        
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillCooldownTimers[i] -= Time.deltaTime;
                characterData.skillCooldowns[i] = skillCooldownTimers[i];
            }
        }
    }
    
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
    
    private void HandleDodge()
    {
        if (Input.GetKeyDown(dodgeKey) && dodgeCooldownTimer <= 0)
        {
            StartCoroutine(Dodge());
        }
    }
    
    private IEnumerator Dodge()
    {
        isDodging = true;
        dodgeCooldownTimer = dodgeCooldown;
        
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
    
    private void HandleAttack()
    {
        if (Input.GetKeyDown(attackKey) && attackCooldownTimer <= 0)
        {
            PerformAttack();
        }
    }
    
    private void PerformAttack()
    {
        attackCooldownTimer = attackCooldown / characterData.totalStats.attackSpeed;
        
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
                    Debug.Log($"暴击！造成 {damage} 点伤害");
                }
                else
                {
                    Debug.Log($"造成 {damage} 点伤害");
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
        
        if (skillCooldownTimers[index] > 0)
        {
            Debug.Log("技能冷却中！");
            return;
        }
        
        if (characterData.currentMana < skill.GetManaCost())
        {
            Debug.Log("魔力不足！");
            return;
        }
        
        characterData.UseMana(Mathf.FloorToInt(skill.GetManaCost()));
        skillCooldownTimers[index] = skill.GetCooldown();
        
        if (animator != null)
        {
            animator.SetTrigger("Skill");
        }
        
        ExecuteSkill(skill);
        
        Debug.Log($"使用了技能：{skill.config.skillName}");
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
                    Debug.Log($"技能造成 {Mathf.FloorToInt(damage)} 点伤害");
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
            Debug.Log($"AOE技能造成 {Mathf.FloorToInt(damage)} 点伤害");
        }
    }
    
    private void ExecuteHealSkill(SkillInstance skill)
    {
        characterData.Heal(Mathf.FloorToInt(skill.config.healAmount));
        Debug.Log($"恢复了 {Mathf.FloorToInt(skill.config.healAmount)} 点生命值");
    }
    
    private void ExecuteBuffSkill(SkillInstance skill)
    {
        Debug.Log($"使用了增益技能：{skill.config.skillName}");
    }
    
    private void ExecuteDebuffSkill(SkillInstance skill)
    {
        Debug.Log($"使用了减益技能：{skill.config.skillName}");
    }
    
    private void ExecuteUtilitySkill(SkillInstance skill)
    {
        Debug.Log($"使用了功能性技能：{skill.config.skillName}");
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
    
    public void TakeDamage(int damage)
    {
        if (isDodging) return;
        
        characterData.TakeDamage(damage);
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        Debug.Log($"受到 {damage} 点伤害");
    }
}
