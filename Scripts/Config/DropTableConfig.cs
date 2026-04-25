using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Drop Table", menuName = "ARPG/Drop Table Config")]
public class DropTableConfig : ScriptableObject
{
    [Header("掉落表名称")]
    public string tableName = "新掉落表";
    
    [Header("掉落物品列表")]
    public List<DropItem> dropItems;
    
    [Header("金币掉落")]
    public int minGoldDrop = 10;
    public int maxGoldDrop = 50;
    public float goldDropChance = 0.8f;
    
    [Header("材料掉落")]
    public List<MaterialDrop> materialDrops;
    
    public List<DropResult> CalculateDrops()
    {
        List<DropResult> results = new List<DropResult>();
        
        if (Random.value < goldDropChance)
        {
            int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);
            results.Add(new DropResult { type = DropType.Gold, amount = goldAmount });
        }
        
        foreach (var materialDrop in materialDrops)
        {
            if (Random.value < materialDrop.dropChance)
            {
                int amount = Random.Range(materialDrop.minAmount, materialDrop.maxAmount + 1);
                results.Add(new DropResult { type = DropType.Material, itemId = materialDrop.materialId, amount = amount });
            }
        }
        
        foreach (var dropItem in dropItems)
        {
            float roll = Random.value;
            if (roll < dropItem.dropChance)
            {
                results.Add(new DropResult { type = DropType.Equipment, itemId = dropItem.equipmentId, amount = 1 });
            }
        }
        
        return results;
    }
}

[System.Serializable]
public class DropItem
{
    public int equipmentId;
    [Range(0f, 1f)]
    public float dropChance = 0.1f;
    [Range(0f, 1f)]
    public float magicFindBonus = 0.1f;
}

[System.Serializable]
public class MaterialDrop
{
    public int materialId;
    public int minAmount = 1;
    public int maxAmount = 3;
    [Range(0f, 1f)]
    public float dropChance = 0.5f;
}

public enum DropType
{
    Gold,
    Equipment,
    Material,
    Consumable
}

public class DropResult
{
    public DropType type;
    public int itemId;
    public int amount;
}
