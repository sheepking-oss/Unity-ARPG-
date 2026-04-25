using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillBarUI : MonoBehaviour
{
    [Header("技能槽位")]
    public List<SkillSlotUI> skillSlots;
    
    [Header("普通攻击")]
    public SkillSlotUI basicAttackSlot;
    
    [Header("技能提示")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipName;
    public TextMeshProUGUI tooltipDescription;
    public TextMeshProUGUI tooltipCooldown;
    public TextMeshProUGUI tooltipManaCost;
    
    [Header("Buff显示")]
    public Transform buffContainer;
    public GameObject buffIconPrefab;
    
    private CharacterData currentData;
    private SkillSlotUI hoveredSlot;
    private int playerId = 0;
    private bool isSubscribed = false;
    private Dictionary<int, BuffIconUI> activeBuffIcons = new Dictionary<int, BuffIconUI>();
    
    private void OnEnable()
    {
        SubscribeToTimerEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromTimerEvents();
    }
    
    private void SubscribeToTimerEvents()
    {
        if (isSubscribed) return;
        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnCooldownsUpdated += OnCooldownsUpdated;
            TimerManager.Instance.OnBuffsUpdated += OnBuffsUpdated;
            TimerManager.Instance.OnSceneChanged += OnSceneChanged;
            isSubscribed = true;
            Debug.Log("[SkillBarUI] 已订阅 TimerManager 事件");
        }
    }
    
    private void UnsubscribeFromTimerEvents()
    {
        if (!isSubscribed) return;
        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnCooldownsUpdated -= OnCooldownsUpdated;
            TimerManager.Instance.OnBuffsUpdated -= OnBuffsUpdated;
            TimerManager.Instance.OnSceneChanged -= OnSceneChanged;
            isSubscribed = false;
            Debug.Log("[SkillBarUI] 已取消订阅 TimerManager 事件");
        }
    }
    
    private void Update()
    {
        if (hoveredSlot != null && tooltipPanel != null)
        {
            UpdateTooltipPosition();
        }
    }
    
    #region 事件处理
    
    private void OnCooldownsUpdated()
    {
        UpdateAllCooldownDisplays();
    }
    
    private void OnBuffsUpdated()
    {
        UpdateBuffDisplay();
    }
    
    private void OnSceneChanged()
    {
        Debug.Log("[SkillBarUI] 场景切换，刷新技能栏显示...");
        
        if (CharacterManager.Instance != null && CharacterManager.Instance.playerCharacter != null)
        {
            UpdateSkills(CharacterManager.Instance.playerCharacter);
        }
        
        UpdateAllCooldownDisplays();
        UpdateBuffDisplay();
    }
    
    #endregion
    
    public void UpdateSkills(CharacterData data)
    {
        if (data == null) return;
        
        currentData = data;
        
        SubscribeToTimerEvents();
        
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i < data.skills.Count)
            {
                SkillInstance skill = data.skills[i];
                skillSlots[i].SetSkill(skill, i);
                skillSlots[i].gameObject.SetActive(true);
            }
            else
            {
                skillSlots[i].ClearSkill();
                skillSlots[i].gameObject.SetActive(false);
            }
        }
        
        if (basicAttackSlot != null)
        {
            basicAttackSlot.SetAsBasicAttack();
        }
        
        UpdateAllCooldownDisplays();
        UpdateBuffDisplay();
        
        Debug.Log($"[SkillBarUI] 技能栏已更新，共 {data.skills.Count} 个技能");
    }
    
    private void UpdateAllCooldownDisplays()
    {
        if (TimerManager.Instance == null) return;
        if (currentData == null) return;
        
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i >= currentData.skills.Count) break;
            if (!skillSlots[i].gameObject.activeSelf) continue;
            
            SkillInstance skill = currentData.skills[i];
            int skillId = skill.config.skillId;
            
            float remaining = TimerManager.Instance.GetCooldownRemaining(skillId, playerId);
            float maxCooldown = skill.GetCooldown();
            
            skillSlots[i].UpdateCooldown(remaining, maxCooldown);
        }
        
        if (basicAttackSlot != null)
        {
            float attackCooldown = TimerManager.Instance.GetCooldownRemaining(PlayerController.ATTACK_SKILL_ID, playerId);
            basicAttackSlot.UpdateCooldown(attackCooldown, 1f);
        }
    }
    
    private void UpdateBuffDisplay()
    {
        if (TimerManager.Instance == null) return;
        if (buffContainer == null) return;
        
        List<BuffData> buffs = TimerManager.Instance.GetAllBuffs(playerId);
        
        HashSet<int> currentBuffIds = new HashSet<int>();
        
        foreach (var buff in buffs)
        {
            currentBuffIds.Add(buff.buffId);
            
            if (activeBuffIcons.ContainsKey(buff.buffId))
            {
                activeBuffIcons[buff.buffId].UpdateBuff(buff);
            }
            else
            {
                if (buffIconPrefab != null)
                {
                    GameObject iconObj = Instantiate(buffIconPrefab, buffContainer);
                    BuffIconUI iconUI = iconObj.GetComponent<BuffIconUI>();
                    
                    if (iconUI != null)
                    {
                        iconUI.Initialize(buff);
                        activeBuffIcons[buff.buffId] = iconUI;
                        Debug.Log($"[SkillBarUI] 创建Buff图标: {buff.buffName} (ID:{buff.buffId})");
                    }
                }
            }
        }
        
        List<int> idsToRemove = new List<int>();
        foreach (var kvp in activeBuffIcons)
        {
            if (!currentBuffIds.Contains(kvp.Key))
            {
                idsToRemove.Add(kvp.Key);
            }
        }
        
        foreach (int id in idsToRemove)
        {
            if (activeBuffIcons.TryGetValue(id, out BuffIconUI icon))
            {
                Destroy(icon.gameObject);
                activeBuffIcons.Remove(id);
                Debug.Log($"[SkillBarUI] 移除Buff图标 (ID:{id})");
            }
        }
    }
    
    #region 提示框系统
    
    public void OnSkillSlotHover(SkillSlotUI slot)
    {
        hoveredSlot = slot;
        ShowTooltip(slot);
    }
    
    public void OnSkillSlotUnhover(SkillSlotUI slot)
    {
        if (hoveredSlot == slot)
        {
            hoveredSlot = null;
            HideTooltip();
        }
    }
    
    private void ShowTooltip(SkillSlotUI slot)
    {
        if (tooltipPanel == null) return;
        
        if (slot.skillInstance != null)
        {
            SkillInstance skill = slot.skillInstance;
            
            if (tooltipName != null)
            {
                tooltipName.text = skill.config.skillName;
            }
            
            if (tooltipDescription != null)
            {
                tooltipDescription.text = skill.config.description;
            }
            
            if (tooltipCooldown != null)
            {
                float remaining = 0f;
                if (TimerManager.Instance != null)
                {
                    remaining = TimerManager.Instance.GetCooldownRemaining(skill.config.skillId, playerId);
                }
                
                if (remaining > 0)
                {
                    tooltipCooldown.text = $"冷却: {remaining:F1}秒 / {skill.GetCooldown():F1}秒";
                }
                else
                {
                    tooltipCooldown.text = $"冷却: {skill.GetCooldown():F1}秒";
                }
            }
            
            if (tooltipManaCost != null)
            {
                tooltipManaCost.text = $"魔力消耗: {skill.GetManaCost()}";
            }
            
            tooltipPanel.SetActive(true);
            UpdateTooltipPosition();
        }
        else if (slot.isBasicAttack)
        {
            if (tooltipName != null)
            {
                tooltipName.text = "普通攻击";
            }
            
            if (tooltipDescription != null)
            {
                tooltipDescription.text = "对敌人造成基础伤害";
            }
            
            if (tooltipCooldown != null)
            {
                float remaining = 0f;
                if (TimerManager.Instance != null)
                {
                    remaining = TimerManager.Instance.GetCooldownRemaining(PlayerController.ATTACK_SKILL_ID, playerId);
                }
                
                if (remaining > 0)
                {
                    tooltipCooldown.text = $"冷却: {remaining:F2}秒";
                }
                else
                {
                    tooltipCooldown.text = "";
                }
            }
            
            if (tooltipManaCost != null)
            {
                tooltipManaCost.text = "";
            }
            
            tooltipPanel.SetActive(true);
            UpdateTooltipPosition();
        }
    }
    
    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    private void UpdateTooltipPosition()
    {
        if (tooltipPanel == null || hoveredSlot == null) return;
        
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        RectTransform slotRect = hoveredSlot.GetComponent<RectTransform>();
        
        if (tooltipRect != null && slotRect != null)
        {
            Vector3[] corners = new Vector3[4];
            slotRect.GetWorldCorners(corners);
            
            float x = corners[2].x + 10f;
            float y = (corners[0].y + corners[1].y) / 2f;
            
            tooltipRect.position = new Vector3(x, y, 0f);
        }
    }
    
    #endregion
    
    #region 调试工具
    
    public string GetDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== SkillBarUI 调试信息 ===");
        sb.AppendLine($"是否订阅事件: {isSubscribed}");
        sb.AppendLine($"玩家ID: {playerId}");
        sb.AppendLine($"活跃Buff数量: {activeBuffIcons.Count}");
        
        if (TimerManager.Instance != null)
        {
            sb.AppendLine($"\n冷却状态 (来自 TimerManager):");
            var cooldowns = TimerManager.Instance.GetAllCooldowns(playerId);
            foreach (var cd in cooldowns)
            {
                sb.AppendLine($"  [{cd.skillId}] {cd.skillName}: {cd.remainingCooldown:F2}s / {cd.totalCooldown:F2}s");
            }
            
            sb.AppendLine($"\nBuff状态 (来自 TimerManager):");
            var buffs = TimerManager.Instance.GetAllBuffs(playerId);
            foreach (var buff in buffs)
            {
                sb.AppendLine($"  [{buff.buffId}] {buff.buffName} ({buff.buffType}): {buff.remainingDuration:F2}s, 层数:{buff.stackCount}");
            }
        }
        
        return sb.ToString();
    }
    
    #endregion
}

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image skillIcon;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI keybindText;
    public TextMeshProUGUI levelText;
    public Button button;
    
    [Header("状态")]
    public SkillInstance skillInstance;
    public int slotIndex;
    public bool isBasicAttack = false;
    public bool isOnCooldown = false;
    
    private SkillBarUI parentBar;
    
    private void Awake()
    {
        parentBar = GetComponentInParent<SkillBarUI>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void SetSkill(SkillInstance skill, int index)
    {
        skillInstance = skill;
        slotIndex = index;
        isBasicAttack = false;
        
        if (skillIcon != null && skill.config.skillIcon != null)
        {
            skillIcon.sprite = skill.config.skillIcon;
            skillIcon.enabled = true;
        }
        
        if (levelText != null)
        {
            levelText.text = $"Lv.{skill.level}";
        }
        
        if (keybindText != null)
        {
            keybindText.text = GetKeybindText(index);
        }
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }
    }
    
    public void SetAsBasicAttack()
    {
        skillInstance = null;
        slotIndex = -1;
        isBasicAttack = true;
        
        if (skillIcon != null)
        {
            skillIcon.enabled = true;
        }
        
        if (levelText != null)
        {
            levelText.text = "";
        }
        
        if (keybindText != null)
        {
            keybindText.text = "LMB";
        }
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }
    }
    
    public void ClearSkill()
    {
        skillInstance = null;
        slotIndex = -1;
        isBasicAttack = false;
        
        if (skillIcon != null)
        {
            skillIcon.enabled = false;
        }
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }
        
        if (cooldownText != null)
        {
            cooldownText.text = "";
        }
        
        if (levelText != null)
        {
            levelText.text = "";
        }
        
        if (keybindText != null)
        {
            keybindText.text = "";
        }
    }
    
    public void UpdateCooldown(float currentCooldown, float maxCooldown)
    {
        if (cooldownOverlay != null)
        {
            if (currentCooldown > 0)
            {
                isOnCooldown = true;
                cooldownOverlay.fillAmount = currentCooldown / maxCooldown;
                cooldownOverlay.enabled = true;
            }
            else
            {
                isOnCooldown = false;
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = false;
            }
        }
        
        if (cooldownText != null)
        {
            if (currentCooldown > 0)
            {
                cooldownText.text = currentCooldown.ToString("F1");
                cooldownText.enabled = true;
            }
            else
            {
                cooldownText.text = "";
                cooldownText.enabled = false;
            }
        }
    }
    
    private string GetKeybindText(int index)
    {
        return (index + 1).ToString();
    }
    
    private void OnClick()
    {
        if (isBasicAttack)
        {
            Debug.Log("[SkillSlotUI] 点击了普通攻击");
        }
        else if (skillInstance != null)
        {
            if (isOnCooldown)
            {
                Debug.Log($"[SkillSlotUI] 技能冷却中：{skillInstance.config.skillName}");
            }
            else
            {
                Debug.Log($"[SkillSlotUI] 点击了技能：{skillInstance.config.skillName}");
            }
        }
    }
    
    private void OnMouseEnter()
    {
        if (parentBar != null)
        {
            parentBar.OnSkillSlotHover(this);
        }
    }
    
    private void OnMouseExit()
    {
        if (parentBar != null)
        {
            parentBar.OnSkillSlotUnhover(this);
        }
    }
}

public class BuffIconUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image buffIcon;
    public Image cooldownOverlay;
    public TextMeshProUGUI stackCountText;
    public TextMeshProUGUI durationText;
    
    [Header("设置")]
    public float tooltipDelay = 0.5f;
    
    private BuffData _buffData;
    private float _hoverTimer;
    private bool _isHovering;
    
    public void Initialize(BuffData buff)
    {
        _buffData = buff;
        UpdateBuff(buff);
    }
    
    public void UpdateBuff(BuffData buff)
    {
        _buffData = buff;
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = buff.GetProgress();
        }
        
        if (stackCountText != null)
        {
            if (buff.stackCount > 1)
            {
                stackCountText.text = buff.stackCount.ToString();
                stackCountText.enabled = true;
            }
            else
            {
                stackCountText.enabled = false;
            }
        }
        
        if (durationText != null)
        {
            if (buff.remainingDuration > 60f)
            {
                durationText.text = $"{Mathf.FloorToInt(buff.remainingDuration / 60f)}m";
            }
            else
            {
                durationText.text = buff.remainingDuration.ToString("F0");
            }
        }
    }
    
    private void Update()
    {
        if (_buffData != null)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = _buffData.GetProgress();
            }
        }
    }
}
