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
    
    private CharacterData currentData;
    private SkillSlotUI hoveredSlot;
    
    private void Update()
    {
        UpdateCooldowns();
        
        if (hoveredSlot != null && tooltipPanel != null)
        {
            UpdateTooltipPosition();
        }
    }
    
    public void UpdateSkills(CharacterData data)
    {
        if (data == null) return;
        
        currentData = data;
        
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
    }
    
    private void UpdateCooldowns()
    {
        if (currentData == null) return;
        
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i < currentData.skills.Count && currentData.skillCooldowns != null && i < currentData.skillCooldowns.Count)
            {
                float cooldown = currentData.skillCooldowns[i];
                float maxCooldown = currentData.skills[i].GetCooldown();
                
                skillSlots[i].UpdateCooldown(cooldown, maxCooldown);
            }
        }
    }
    
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
                tooltipCooldown.text = $"冷却: {skill.GetCooldown():F1}秒";
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
                tooltipCooldown.text = "";
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
}

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image skillIcon;
    public Image cooldownOverlay;
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
        if (cooldownOverlay == null) return;
        
        if (currentCooldown > 0)
        {
            isOnCooldown = true;
            cooldownOverlay.fillAmount = currentCooldown / maxCooldown;
        }
        else
        {
            isOnCooldown = false;
            cooldownOverlay.fillAmount = 0f;
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
            Debug.Log("点击了普通攻击");
        }
        else if (skillInstance != null && !isOnCooldown)
        {
            Debug.Log($"点击了技能：{skillInstance.config.skillName}");
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
