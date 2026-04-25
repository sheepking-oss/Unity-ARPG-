using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EquipmentPanelUI : MonoBehaviour
{
    [Header("装备槽位")]
    public List<EquipmentSlotUI> equipmentSlots;
    
    [Header("装备提示")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipName;
    public TextMeshProUGUI tooltipDescription;
    public TextMeshProUGUI tooltipStats;
    public TextMeshProUGUI tooltipQuality;
    public TextMeshProUGUI tooltipLevel;
    
    [Header("按钮")]
    public Button closeButton;
    public Button toggleButton;
    
    [Header("面板")]
    public GameObject equipmentPanel;
    
    private CharacterData currentData;
    private EquipmentSlotUI hoveredSlot;
    private bool isVisible = false;
    
    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(Toggle);
        }
        
        Hide();
    }
    
    private void Update()
    {
        if (hoveredSlot != null && tooltipPanel != null)
        {
            UpdateTooltipPosition();
        }
    }
    
    public void UpdateEquipment(CharacterData data)
    {
        if (data == null) return;
        
        currentData = data;
        
        foreach (var slot in equipmentSlots)
        {
            if (data.equippedItems.TryGetValue(slot.slot, out EquipmentInstance item))
            {
                slot.SetEquipment(item);
            }
            else
            {
                slot.ClearEquipment();
            }
        }
    }
    
    public void OnSlotHover(EquipmentSlotUI slot)
    {
        hoveredSlot = slot;
        ShowTooltip(slot);
    }
    
    public void OnSlotUnhover(EquipmentSlotUI slot)
    {
        if (hoveredSlot == slot)
        {
            hoveredSlot = null;
            HideTooltip();
        }
    }
    
    public void OnSlotClick(EquipmentSlotUI slot)
    {
        if (slot.equipmentInstance != null)
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.UnequipItem(slot.slot);
            }
        }
    }
    
    private void ShowTooltip(EquipmentSlotUI slot)
    {
        if (tooltipPanel == null || slot.equipmentInstance == null) return;
        
        EquipmentInstance equipment = slot.equipmentInstance;
        EquipmentConfig config = equipment.config;
        
        if (tooltipName != null)
        {
            tooltipName.text = EquipmentManager.Instance?.GetEquipmentFullName(equipment) ?? config.equipmentName;
            tooltipName.color = EquipmentManager.Instance?.GetQualityColor(config.quality) ?? Color.white;
        }
        
        if (tooltipDescription != null)
        {
            tooltipDescription.text = config.description;
        }
        
        if (tooltipQuality != null)
        {
            tooltipQuality.text = GetQualityText(config.quality);
            tooltipQuality.color = EquipmentManager.Instance?.GetQualityColor(config.quality) ?? Color.white;
        }
        
        if (tooltipLevel != null)
        {
            tooltipLevel.text = $"需要等级: {config.requiredLevel}";
            
            if (currentData != null && currentData.level < config.requiredLevel)
            {
                tooltipLevel.color = Color.red;
            }
            else
            {
                tooltipLevel.color = Color.white;
            }
        }
        
        if (tooltipStats != null)
        {
            tooltipStats.text = GetStatsText(equipment);
        }
        
        tooltipPanel.SetActive(true);
        UpdateTooltipPosition();
    }
    
    private string GetQualityText(EquipmentQuality quality)
    {
        switch (quality)
        {
            case EquipmentQuality.Common:
                return "普通";
            case EquipmentQuality.Uncommon:
                return "优秀";
            case EquipmentQuality.Rare:
                return "稀有";
            case EquipmentQuality.Epic:
                return "史诗";
            case EquipmentQuality.Legendary:
                return "传说";
            default:
                return "";
        }
    }
    
    private string GetStatsText(EquipmentInstance equipment)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        EquipmentConfig config = equipment.config;
        
        sb.AppendLine("基础属性:");
        if (config.baseStats.maxHealth > 0)
            sb.AppendLine($"  生命值: +{config.baseStats.maxHealth}");
        if (config.baseStats.maxMana > 0)
            sb.AppendLine($"  魔力: +{config.baseStats.maxMana}");
        if (config.baseStats.strength > 0)
            sb.AppendLine($"  力量: +{config.baseStats.strength}");
        if (config.baseStats.intelligence > 0)
            sb.AppendLine($"  智力: +{config.baseStats.intelligence}");
        if (config.baseStats.agility > 0)
            sb.AppendLine($"  敏捷: +{config.baseStats.agility}");
        if (config.baseStats.vitality > 0)
            sb.AppendLine($"  体力: +{config.baseStats.vitality}");
        if (config.baseStats.attackPower > 0)
            sb.AppendLine($"  攻击力: +{config.baseStats.attackPower}");
        if (config.baseStats.defense > 0)
            sb.AppendLine($"  防御力: +{config.baseStats.defense}");
        
        if (equipment.prefixes.Count > 0)
        {
            sb.AppendLine("\n前缀:");
            foreach (var prefix in equipment.prefixes)
            {
                sb.AppendLine($"  {prefix.config.affixName}");
            }
        }
        
        if (equipment.suffixes.Count > 0)
        {
            sb.AppendLine("\n后缀:");
            foreach (var suffix in equipment.suffixes)
            {
                sb.AppendLine($"  {suffix.config.affixName}");
            }
        }
        
        if (equipment.enhanceLevel > 0)
        {
            sb.AppendLine($"\n强化等级: +{equipment.enhanceLevel}");
        }
        
        return sb.ToString();
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
    
    public void Show()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(true);
            isVisible = true;
        }
    }
    
    public void Hide()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
            isVisible = false;
        }
    }
    
    public void Toggle()
    {
        if (isVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
}

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image equipmentIcon;
    public Image qualityBorder;
    public TextMeshProUGUI enhanceLevelText;
    public Button button;
    
    [Header("槽位信息")]
    public EquipmentSlot slot;
    
    [Header("状态")]
    public EquipmentInstance equipmentInstance;
    
    private EquipmentPanelUI parentPanel;
    
    private void Awake()
    {
        parentPanel = GetComponentInParent<EquipmentPanelUI>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void SetEquipment(EquipmentInstance equipment)
    {
        equipmentInstance = equipment;
        
        if (equipmentIcon != null && equipment.config.equipmentIcon != null)
        {
            equipmentIcon.sprite = equipment.config.equipmentIcon;
            equipmentIcon.enabled = true;
        }
        
        if (qualityBorder != null)
        {
            qualityBorder.color = EquipmentManager.Instance?.GetQualityColor(equipment.config.quality) ?? Color.white;
            qualityBorder.enabled = true;
        }
        
        if (enhanceLevelText != null && equipment.enhanceLevel > 0)
        {
            enhanceLevelText.text = $"+{equipment.enhanceLevel}";
            enhanceLevelText.enabled = true;
        }
        else if (enhanceLevelText != null)
        {
            enhanceLevelText.enabled = false;
        }
    }
    
    public void ClearEquipment()
    {
        equipmentInstance = null;
        
        if (equipmentIcon != null)
        {
            equipmentIcon.enabled = false;
        }
        
        if (qualityBorder != null)
        {
            qualityBorder.enabled = false;
        }
        
        if (enhanceLevelText != null)
        {
            enhanceLevelText.enabled = false;
        }
    }
    
    private void OnClick()
    {
        if (parentPanel != null)
        {
            parentPanel.OnSlotClick(this);
        }
    }
    
    private void OnMouseEnter()
    {
        if (parentPanel != null)
        {
            parentPanel.OnSlotHover(this);
        }
    }
    
    private void OnMouseExit()
    {
        if (parentPanel != null)
        {
            parentPanel.OnSlotUnhover(this);
        }
    }
}
