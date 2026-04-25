using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("背包槽位")]
    public List<InventorySlotUI> inventorySlots;
    public int maxSlots = 30;
    
    [Header("物品提示")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipName;
    public TextMeshProUGUI tooltipDescription;
    public TextMeshProUGUI tooltipStats;
    public TextMeshProUGUI tooltipQuality;
    public TextMeshProUGUI tooltipLevel;
    public TextMeshProUGUI tooltipPrice;
    
    [Header("操作按钮")]
    public Button equipButton;
    public Button sellButton;
    public Button dropButton;
    
    [Header("导航按钮")]
    public Button closeButton;
    public Button toggleButton;
    
    [Header("面板")]
    public GameObject inventoryPanel;
    
    [Header("金币显示")]
    public TextMeshProUGUI goldText;
    
    private CharacterData currentData;
    private InventorySlotUI selectedSlot;
    private InventorySlotUI hoveredSlot;
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
        
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipButtonClick);
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClick);
        }
        
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClick);
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
    
    public void UpdateInventory(CharacterData data)
    {
        if (data == null) return;
        
        currentData = data;
        
        if (goldText != null)
        {
            goldText.text = $"{data.gold}";
        }
        
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < data.inventory.Count)
            {
                inventorySlots[i].SetItem(data.inventory[i]);
                inventorySlots[i].gameObject.SetActive(true);
            }
            else
            {
                inventorySlots[i].ClearItem();
                inventorySlots[i].gameObject.SetActive(i < maxSlots);
            }
        }
    }
    
    public void OnSlotSelect(InventorySlotUI slot)
    {
        if (selectedSlot != null)
        {
            selectedSlot.Deselect();
        }
        
        selectedSlot = slot;
        slot.Select();
        
        UpdateActionButtons();
    }
    
    public void OnSlotHover(InventorySlotUI slot)
    {
        hoveredSlot = slot;
        ShowTooltip(slot);
    }
    
    public void OnSlotUnhover(InventorySlotUI slot)
    {
        if (hoveredSlot == slot)
        {
            hoveredSlot = null;
            HideTooltip();
        }
    }
    
    private void UpdateActionButtons()
    {
        if (selectedSlot == null || selectedSlot.itemInstance == null)
        {
            if (equipButton != null) equipButton.interactable = false;
            if (sellButton != null) sellButton.interactable = false;
            if (dropButton != null) dropButton.interactable = false;
            return;
        }
        
        EquipmentInstance equipment = selectedSlot.itemInstance as EquipmentInstance;
        
        if (equipButton != null)
        {
            equipButton.interactable = equipment != null;
        }
        
        if (sellButton != null)
        {
            sellButton.interactable = equipment != null;
        }
        
        if (dropButton != null)
        {
            dropButton.interactable = true;
        }
    }
    
    private void OnEquipButtonClick()
    {
        if (selectedSlot == null || selectedSlot.itemInstance == null) return;
        
        EquipmentInstance equipment = selectedSlot.itemInstance as EquipmentInstance;
        if (equipment != null)
        {
            if (CharacterManager.Instance != null)
            {
                bool success = CharacterManager.Instance.EquipItem(equipment);
                if (success)
                {
                    selectedSlot.ClearItem();
                    selectedSlot = null;
                    UpdateActionButtons();
                }
            }
        }
    }
    
    private void OnSellButtonClick()
    {
        if (selectedSlot == null || selectedSlot.itemInstance == null) return;
        
        EquipmentInstance equipment = selectedSlot.itemInstance as EquipmentInstance;
        if (equipment != null && ShopManager.Instance != null)
        {
            int sellPrice = equipment.config.sellPrice;
            
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.AddGold(sellPrice);
                CharacterManager.Instance.RemoveFromInventory(equipment);
                
                selectedSlot.ClearItem();
                selectedSlot = null;
                UpdateActionButtons();
                
                Debug.Log($"出售了 {equipment.config.equipmentName}，获得 {sellPrice} 金币");
            }
        }
    }
    
    private void OnDropButtonClick()
    {
        if (selectedSlot == null || selectedSlot.itemInstance == null) return;
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.RemoveFromInventory(selectedSlot.itemInstance as EquipmentInstance);
            
            Debug.Log($"丢弃了物品");
            
            selectedSlot.ClearItem();
            selectedSlot = null;
            UpdateActionButtons();
        }
    }
    
    private void ShowTooltip(InventorySlotUI slot)
    {
        if (tooltipPanel == null || slot.itemInstance == null) return;
        
        EquipmentInstance equipment = slot.itemInstance as EquipmentInstance;
        if (equipment == null) return;
        
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
        
        if (tooltipPrice != null)
        {
            tooltipPrice.text = $"出售价格: {config.sellPrice} 金币";
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
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            isVisible = true;
        }
    }
    
    public void Hide()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
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

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image itemIcon;
    public Image qualityBorder;
    public TextMeshProUGUI countText;
    public Button button;
    public Image selectionHighlight;
    
    [Header("状态")]
    public EquipmentInstance itemInstance;
    public bool isSelected = false;
    
    private InventoryUI parentInventory;
    
    private void Awake()
    {
        parentInventory = GetComponentInParent<InventoryUI>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void SetItem(EquipmentInstance item)
    {
        itemInstance = item;
        
        if (itemIcon != null && item.config.equipmentIcon != null)
        {
            itemIcon.sprite = item.config.equipmentIcon;
            itemIcon.enabled = true;
        }
        
        if (qualityBorder != null)
        {
            qualityBorder.color = EquipmentManager.Instance?.GetQualityColor(item.config.quality) ?? Color.white;
            qualityBorder.enabled = true;
        }
        
        if (countText != null)
        {
            countText.enabled = false;
        }
    }
    
    public void ClearItem()
    {
        itemInstance = null;
        
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }
        
        if (qualityBorder != null)
        {
            qualityBorder.enabled = false;
        }
        
        if (countText != null)
        {
            countText.enabled = false;
        }
        
        Deselect();
    }
    
    public void Select()
    {
        isSelected = true;
        if (selectionHighlight != null)
        {
            selectionHighlight.enabled = true;
        }
    }
    
    public void Deselect()
    {
        isSelected = false;
        if (selectionHighlight != null)
        {
            selectionHighlight.enabled = false;
        }
    }
    
    private void OnClick()
    {
        if (parentInventory != null && itemInstance != null)
        {
            parentInventory.OnSlotSelect(this);
        }
    }
    
    private void OnMouseEnter()
    {
        if (parentInventory != null && itemInstance != null)
        {
            parentInventory.OnSlotHover(this);
        }
    }
    
    private void OnMouseExit()
    {
        if (parentInventory != null)
        {
            parentInventory.OnSlotUnhover(this);
        }
    }
}
