using UnityEngine;
using System.Collections.Generic;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }
    
    [Header("掉落设置")]
    public float pickupRange = 2f;
    public float autoPickupDelay = 2f;
    public bool autoPickupGold = true;
    public bool autoPickupMaterials = true;
    
    [Header("掉落物品预制体")]
    public GameObject goldPickupPrefab;
    public GameObject equipmentPickupPrefab;
    public GameObject materialPickupPrefab;
    
    [Header("特效")]
    public GameObject pickupEffect;
    
    [Header("当前状态")]
    public List<LootPickup> activePickups;
    
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
        CheckAutoPickup();
    }
    
    private void CheckAutoPickup()
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.currentPlayer == null) return;
        
        Transform playerTransform = CharacterManager.Instance.currentPlayer.transform;
        
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            LootPickup pickup = activePickups[i];
            
            if (pickup == null)
            {
                activePickups.RemoveAt(i);
                continue;
            }
            
            float distance = Vector3.Distance(playerTransform.position, pickup.transform.position);
            
            if (distance <= pickupRange)
            {
                if (ShouldAutoPickup(pickup))
                {
                    pickup.autoPickupTimer -= Time.deltaTime;
                    
                    if (pickup.autoPickupTimer <= 0)
                    {
                        PickupItem(pickup);
                    }
                }
            }
        }
    }
    
    private bool ShouldAutoPickup(LootPickup pickup)
    {
        switch (pickup.lootType)
        {
            case LootType.Gold:
                return autoPickupGold;
            case LootType.Material:
                return autoPickupMaterials;
            case LootType.Equipment:
            case LootType.Consumable:
            default:
                return false;
        }
    }
    
    public void SpawnLootFromDropResult(List<DropResult> drops, Vector3 position)
    {
        if (drops == null || drops.Count == 0) return;
        
        foreach (var drop in drops)
        {
            SpawnLoot(drop, position + Random.insideUnitSphere * 1f);
        }
    }
    
    public void SpawnLoot(DropResult drop, Vector3 position)
    {
        GameObject pickupObject = null;
        
        switch (drop.type)
        {
            case LootType.Gold:
                pickupObject = SpawnGoldPickup(drop.amount, position);
                break;
            case LootType.Equipment:
                pickupObject = SpawnEquipmentPickup(drop.itemId, position);
                break;
            case LootType.Material:
                pickupObject = SpawnMaterialPickup(drop.itemId, drop.amount, position);
                break;
            case LootType.Consumable:
                pickupObject = SpawnConsumablePickup(drop.itemId, drop.amount, position);
                break;
        }
        
        if (pickupObject != null)
        {
            LootPickup pickup = pickupObject.GetComponent<LootPickup>();
            if (pickup != null)
            {
                pickup.autoPickupTimer = autoPickupDelay;
                activePickups.Add(pickup);
            }
        }
    }
    
    private GameObject SpawnGoldPickup(int amount, Vector3 position)
    {
        if (goldPickupPrefab == null) return null;
        
        GameObject pickupObject = Instantiate(goldPickupPrefab, position, Quaternion.identity);
        GoldPickup goldPickup = pickupObject.GetComponent<GoldPickup>();
        
        if (goldPickup != null)
        {
            goldPickup.amount = amount;
            goldPickup.lootType = LootType.Gold;
        }
        
        return pickupObject;
    }
    
    private GameObject SpawnEquipmentPickup(int equipmentId, Vector3 position)
    {
        if (equipmentPickupPrefab == null) return null;
        
        EquipmentInstance equipment = EquipmentGenerator.Instance?.GenerateEquipment(equipmentId);
        if (equipment == null) return null;
        
        GameObject pickupObject = Instantiate(equipmentPickupPrefab, position, Quaternion.identity);
        EquipmentPickup equipmentPickup = pickupObject.GetComponent<EquipmentPickup>();
        
        if (equipmentPickup != null)
        {
            equipmentPickup.equipment = equipment;
            equipmentPickup.lootType = LootType.Equipment;
        }
        
        return pickupObject;
    }
    
    private GameObject SpawnMaterialPickup(int materialId, int amount, Vector3 position)
    {
        if (materialPickupPrefab == null) return null;
        
        GameObject pickupObject = Instantiate(materialPickupPrefab, position, Quaternion.identity);
        MaterialPickup materialPickup = pickupObject.GetComponent<MaterialPickup>();
        
        if (materialPickup != null)
        {
            materialPickup.materialId = materialId;
            materialPickup.amount = amount;
            materialPickup.lootType = LootType.Material;
        }
        
        return pickupObject;
    }
    
    private GameObject SpawnConsumablePickup(int itemId, int amount, Vector3 position)
    {
        Debug.Log($"生成消耗品：ID={itemId}，数量={amount}");
        return null;
    }
    
    public void PickupItem(LootPickup pickup)
    {
        if (pickup == null) return;
        
        switch (pickup.lootType)
        {
            case LootType.Gold:
                GoldPickup goldPickup = pickup as GoldPickup;
                if (goldPickup != null && CharacterManager.Instance != null)
                {
                    CharacterManager.Instance.AddGold(goldPickup.amount);
                    ShowPickupMessage($"获得金币：{goldPickup.amount}");
                }
                break;
                
            case LootType.Equipment:
                EquipmentPickup equipmentPickup = pickup as EquipmentPickup;
                if (equipmentPickup != null && equipmentPickup.equipment != null && CharacterManager.Instance != null)
                {
                    CharacterManager.Instance.AddToInventory(equipmentPickup.equipment);
                    ShowPickupMessage($"获得装备：{EquipmentManager.Instance?.GetEquipmentFullName(equipmentPickup.equipment)}");
                }
                break;
                
            case LootType.Material:
                MaterialPickup materialPickup = pickup as MaterialPickup;
                if (materialPickup != null)
                {
                    ShowPickupMessage($"获得材料：ID={materialPickup.materialId}，数量={materialPickup.amount}");
                }
                break;
        }
        
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, pickup.transform.position, Quaternion.identity);
        }
        
        activePickups.Remove(pickup);
        Destroy(pickup.gameObject);
    }
    
    private void ShowPickupMessage(string message)
    {
        Debug.Log(message);
    }
    
    public void ForcePickupAll()
    {
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            LootPickup pickup = activePickups[i];
            if (pickup != null)
            {
                PickupItem(pickup);
            }
        }
    }
    
    public void ClearAllPickups()
    {
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            LootPickup pickup = activePickups[i];
            if (pickup != null)
            {
                Destroy(pickup.gameObject);
            }
        }
        activePickups.Clear();
    }
}

public class LootPickup : MonoBehaviour
{
    public LootType lootType;
    public float autoPickupTimer;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LootManager.Instance?.PickupItem(this);
        }
    }
}

public class GoldPickup : LootPickup
{
    public int amount;
}

public class EquipmentPickup : LootPickup
{
    public EquipmentInstance equipment;
}

public class MaterialPickup : LootPickup
{
    public int materialId;
    public int amount;
}
