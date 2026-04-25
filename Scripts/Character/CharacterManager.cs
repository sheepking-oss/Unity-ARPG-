using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    
    [Header("职业选择")]
    public List<ClassConfig> availableClasses;
    public ClassConfig selectedClass;
    
    [Header("角色数据")]
    public CharacterData playerCharacter;
    public GameObject playerPrefab;
    public GameObject currentPlayer;
    
    [Header("UI引用")]
    public CharacterStatsUI statsUI;
    public SkillBarUI skillBarUI;
    public EquipmentPanelUI equipmentPanelUI;
    public InventoryUI inventoryUI;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public void SelectClass(ClassConfig classConfig)
    {
        selectedClass = classConfig;
        Debug.Log($"选择了职业：{classConfig.className}");
    }
    
    public void CreateCharacter()
    {
        if (selectedClass == null)
        {
            Debug.LogError("请先选择职业！");
            return;
        }
        
        playerCharacter = new CharacterData();
        playerCharacter.Initialize(selectedClass);
        
        Debug.Log($"创建了角色：{playerCharacter.characterName}，等级：{playerCharacter.level}");
    }
    
    public void SpawnPlayer(Vector3 position)
    {
        if (playerCharacter == null)
        {
            Debug.LogError("没有创建角色！");
            return;
        }
        
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }
        
        currentPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        
        PlayerController controller = currentPlayer.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.Initialize(playerCharacter);
        }
        
        UpdateAllUI();
        
        Debug.Log("玩家已生成");
    }
    
    public void AddExperience(int amount)
    {
        if (playerCharacter == null) return;
        
        int previousLevel = playerCharacter.level;
        playerCharacter.AddExperience(amount);
        
        if (playerCharacter.level > previousLevel)
        {
            UpdateAllUI();
        }
        
        Debug.Log($"获得 {amount} 点经验值");
    }
    
    public void AddGold(int amount)
    {
        if (playerCharacter == null) return;
        
        playerCharacter.gold += amount;
        UpdateAllUI();
        
        Debug.Log($"获得 {amount} 金币");
    }
    
    public bool SpendGold(int amount)
    {
        if (playerCharacter == null) return false;
        
        if (playerCharacter.gold >= amount)
        {
            playerCharacter.gold -= amount;
            UpdateAllUI();
            return true;
        }
        
        Debug.Log("金币不足！");
        return false;
    }
    
    public bool EquipItem(EquipmentInstance item)
    {
        if (playerCharacter == null) return false;
        
        bool success = playerCharacter.EquipItem(item);
        if (success)
        {
            UpdateAllUI();
        }
        
        return success;
    }
    
    public void UnequipItem(EquipmentSlot slot)
    {
        if (playerCharacter == null) return;
        
        playerCharacter.UnequipItem(slot);
        UpdateAllUI();
    }
    
    public void AddToInventory(EquipmentInstance item)
    {
        if (playerCharacter == null) return;
        
        playerCharacter.inventory.Add(item);
        UpdateAllUI();
    }
    
    public void RemoveFromInventory(EquipmentInstance item)
    {
        if (playerCharacter == null) return;
        
        playerCharacter.inventory.Remove(item);
        UpdateAllUI();
    }
    
    public void UpdateAllUI()
    {
        if (statsUI != null)
        {
            statsUI.UpdateStats(playerCharacter);
        }
        
        if (skillBarUI != null)
        {
            skillBarUI.UpdateSkills(playerCharacter);
        }
        
        if (equipmentPanelUI != null)
        {
            equipmentPanelUI.UpdateEquipment(playerCharacter);
        }
        
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventory(playerCharacter);
        }
    }
    
    public CharacterStats GetPlayerStats()
    {
        return playerCharacter != null ? playerCharacter.totalStats : new CharacterStats();
    }
}
