using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    [Header("商店配置")]
    public List<ShopItem> shopItems;
    public float priceMultiplier = 1.0f;
    
    [Header("UI引用")]
    public GameObject shopPanel;
    public List<ShopItemUI> shopItemSlots;
    public TextMeshProUGUI goldText;
    public Button closeButton;
    public Button refreshButton;
    
    [Header("刷新设置")]
    public int refreshCost = 100;
    public float refreshInterval = 300f;
    
    private float lastRefreshTime = 0f;
    private bool isOpen = false;
    
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
    
    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshShop);
        }
        
        GenerateShopItems();
    }
    
    private void Update()
    {
        if (isOpen)
        {
            UpdateGoldDisplay();
        }
    }
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            isOpen = true;
        }
        
        UpdateShopUI();
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            isOpen = false;
        }
    }
    
    public void ToggleShop()
    {
        if (isOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }
    
    private void GenerateShopItems()
    {
        if (shopItems == null || shopItems.Count == 0)
        {
            GenerateDefaultShopItems();
        }
        
        UpdateShopUI();
    }
    
    private void GenerateDefaultShopItems()
    {
        shopItems = new List<ShopItem>();
        
        if (EquipmentManager.Instance != null && EquipmentManager.Instance.allEquipment != null)
        {
            List<EquipmentConfig> commonEquipment = EquipmentManager.Instance.GetEquipmentByQuality(EquipmentQuality.Common);
            List<EquipmentConfig> uncommonEquipment = EquipmentManager.Instance.GetEquipmentByQuality(EquipmentQuality.Uncommon);
            
            foreach (var equipment in commonEquipment)
            {
                shopItems.Add(new ShopItem
                {
                    equipmentId = equipment.equipmentId,
                    buyPrice = Mathf.FloorToInt(equipment.buyPrice * priceMultiplier),
                    stock = Random.Range(1, 5)
                });
            }
            
            foreach (var equipment in uncommonEquipment)
            {
                shopItems.Add(new ShopItem
                {
                    equipmentId = equipment.equipmentId,
                    buyPrice = Mathf.FloorToInt(equipment.buyPrice * priceMultiplier),
                    stock = Random.Range(1, 3)
                });
            }
        }
    }
    
    private void UpdateShopUI()
    {
        UpdateGoldDisplay();
        
        for (int i = 0; i < shopItemSlots.Count; i++)
        {
            if (i < shopItems.Count)
            {
                ShopItem item = shopItems[i];
                EquipmentConfig config = EquipmentManager.Instance?.GetEquipmentById(item.equipmentId);
                
                if (config != null)
                {
                    shopItemSlots[i].SetItem(config, item);
                    shopItemSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    shopItemSlots[i].ClearItem();
                    shopItemSlots[i].gameObject.SetActive(false);
                }
            }
            else
            {
                shopItemSlots[i].ClearItem();
                shopItemSlots[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateGoldDisplay()
    {
        if (goldText != null && CharacterManager.Instance != null)
        {
            goldText.text = $"{CharacterManager.Instance.playerCharacter.gold}";
        }
    }
    
    public void BuyItem(ShopItemUI itemUI)
    {
        if (itemUI == null || itemUI.shopItem == null) return;
        
        ShopItem item = itemUI.shopItem;
        
        if (item.stock <= 0)
        {
            Debug.Log("库存不足！");
            return;
        }
        
        if (CharacterManager.Instance == null) return;
        
        if (!CharacterManager.Instance.SpendGold(item.buyPrice))
        {
            Debug.Log("金币不足！");
            return;
        }
        
        EquipmentInstance equipment = EquipmentGenerator.Instance?.GenerateEquipment(item.equipmentId);
        if (equipment != null)
        {
            CharacterManager.Instance.AddToInventory(equipment);
            item.stock--;
            
            Debug.Log($"购买了 {equipment.config.equipmentName}，花费 {item.buyPrice} 金币");
            
            UpdateShopUI();
        }
    }
    
    public void SellItem(EquipmentInstance equipment)
    {
        if (equipment == null || equipment.config == null) return;
        
        int sellPrice = equipment.config.sellPrice;
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.AddGold(sellPrice);
            CharacterManager.Instance.RemoveFromInventory(equipment);
            
            Debug.Log($"出售了 {equipment.config.equipmentName}，获得 {sellPrice} 金币");
        }
    }
    
    public void RefreshShop()
    {
        if (CharacterManager.Instance != null)
        {
            if (!CharacterManager.Instance.SpendGold(refreshCost))
            {
                Debug.Log("金币不足，无法刷新商店！");
                return;
            }
        }
        
        GenerateDefaultShopItems();
        lastRefreshTime = Time.time;
        
        Debug.Log("商店已刷新！");
    }
    
    public bool CanRefresh()
    {
        return Time.time - lastRefreshTime >= refreshInterval;
    }
}

[System.Serializable]
public class ShopItem
{
    public int equipmentId;
    public int buyPrice;
    public int stock;
}

public class ShopItemUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image itemIcon;
    public Image qualityBorder;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    public Button buyButton;
    
    [Header("状态")]
    public EquipmentConfig equipmentConfig;
    public ShopItem shopItem;
    
    private ShopManager parentShop;
    
    private void Awake()
    {
        parentShop = GetComponentInParent<ShopManager>();
        
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClick);
        }
    }
    
    public void SetItem(EquipmentConfig config, ShopItem item)
    {
        equipmentConfig = config;
        shopItem = item;
        
        if (itemIcon != null && config.equipmentIcon != null)
        {
            itemIcon.sprite = config.equipmentIcon;
            itemIcon.enabled = true;
        }
        
        if (qualityBorder != null)
        {
            qualityBorder.color = EquipmentManager.Instance?.GetQualityColor(config.quality) ?? Color.white;
            qualityBorder.enabled = true;
        }
        
        if (itemName != null)
        {
            itemName.text = config.equipmentName;
            itemName.color = EquipmentManager.Instance?.GetQualityColor(config.quality) ?? Color.white;
        }
        
        if (priceText != null)
        {
            priceText.text = $"{item.buyPrice}";
        }
        
        if (stockText != null)
        {
            stockText.text = $"x{item.stock}";
        }
        
        if (buyButton != null)
        {
            buyButton.interactable = item.stock > 0;
        }
    }
    
    public void ClearItem()
    {
        equipmentConfig = null;
        shopItem = null;
        
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }
        
        if (qualityBorder != null)
        {
            qualityBorder.enabled = false;
        }
        
        if (itemName != null)
        {
            itemName.text = "";
        }
        
        if (priceText != null)
        {
            priceText.text = "";
        }
        
        if (stockText != null)
        {
            stockText.text = "";
        }
        
        if (buyButton != null)
        {
            buyButton.interactable = false;
        }
    }
    
    private void OnBuyButtonClick()
    {
        if (parentShop != null)
        {
            parentShop.BuyItem(this);
        }
    }
}
