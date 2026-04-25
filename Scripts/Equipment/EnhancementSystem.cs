using UnityEngine;

public class EnhancementSystem : MonoBehaviour
{
    public static EnhancementSystem Instance { get; private set; }
    
    [Header("强化设置")]
    public int baseEnhanceCost = 100;
    public float costMultiplier = 1.5f;
    
    [Header("成功率")]
    [Range(0f, 1f)]
    public float baseSuccessRate = 0.9f;
    public float successRateDecrease = 0.05f;
    [Range(0f, 1f)]
    public float minSuccessRate = 0.1f;
    
    [Header("强化保护")]
    public bool enableProtection = true;
    public int protectionLevelThreshold = 5;
    public int destroyAfterFailLevel = 10;
    
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
    
    public EnhancementResult EnhanceEquipment(EquipmentInstance equipment)
    {
        if (equipment == null || equipment.config == null)
        {
            return EnhancementResult.InvalidEquipment;
        }
        
        if (equipment.enhanceLevel >= equipment.config.maxEnhanceLevel)
        {
            return EnhancementResult.MaxLevelReached;
        }
        
        int enhanceCost = GetEnhanceCost(equipment);
        
        if (CharacterManager.Instance != null)
        {
            if (!CharacterManager.Instance.SpendGold(enhanceCost))
            {
                return EnhancementResult.NotEnoughGold;
            }
        }
        else
        {
            return EnhancementResult.NotEnoughGold;
        }
        
        float successRate = GetSuccessRate(equipment.enhanceLevel);
        bool success = Random.value < successRate;
        
        if (success)
        {
            equipment.enhanceLevel++;
            
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.playerCharacter.CalculateTotalStats();
                CharacterManager.Instance.UpdateAllUI();
            }
            
            Debug.Log($"强化成功！{equipment.config.equipmentName} 现在是 +{equipment.enhanceLevel}");
            return EnhancementResult.Success;
        }
        else
        {
            return HandleEnhancementFailure(equipment);
        }
    }
    
    private EnhancementResult HandleEnhancementFailure(EquipmentInstance equipment)
    {
        if (equipment.enhanceLevel <= protectionLevelThreshold && enableProtection)
        {
            Debug.Log($"强化失败，但 {equipment.config.equipmentName} 保持原样（保护等级内）");
            return EnhancementResult.FailureNoPenalty;
        }
        
        if (equipment.enhanceLevel >= destroyAfterFailLevel)
        {
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.RemoveFromInventory(equipment);
            }
            
            Debug.Log($"强化失败！{equipment.config.equipmentName} 被摧毁了！");
            return EnhancementResult.Destroyed;
        }
        
        equipment.enhanceLevel = Mathf.Max(0, equipment.enhanceLevel - 1);
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.playerCharacter.CalculateTotalStats();
            CharacterManager.Instance.UpdateAllUI();
        }
        
        Debug.Log($"强化失败！{equipment.config.equipmentName} 降级到 +{equipment.enhanceLevel}");
        return EnhancementResult.FailureDowngrade;
    }
    
    public int GetEnhanceCost(EquipmentInstance equipment)
    {
        if (equipment == null || equipment.config == null)
        {
            return 0;
        }
        
        float cost = equipment.config.enhanceCostPerLevel;
        cost *= Mathf.Pow(costMultiplier, equipment.enhanceLevel);
        
        return Mathf.FloorToInt(cost);
    }
    
    public float GetSuccessRate(int currentLevel)
    {
        float successRate = baseSuccessRate - (currentLevel * successRateDecrease);
        return Mathf.Max(minSuccessRate, successRate);
    }
    
    public bool CanEnhance(EquipmentInstance equipment)
    {
        if (equipment == null || equipment.config == null)
        {
            return false;
        }
        
        if (equipment.enhanceLevel >= equipment.config.maxEnhanceLevel)
        {
            return false;
        }
        
        int cost = GetEnhanceCost(equipment);
        
        if (CharacterManager.Instance != null)
        {
            return CharacterManager.Instance.playerCharacter.gold >= cost;
        }
        
        return false;
    }
    
    public EnhancementResult EnhanceEquipmentWithProtection(EquipmentInstance equipment, bool useProtectionScroll)
    {
        if (!useProtectionScroll)
        {
            return EnhanceEquipment(equipment);
        }
        
        if (equipment == null || equipment.config == null)
        {
            return EnhancementResult.InvalidEquipment;
        }
        
        if (equipment.enhanceLevel >= equipment.config.maxEnhanceLevel)
        {
            return EnhancementResult.MaxLevelReached;
        }
        
        int enhanceCost = GetEnhanceCost(equipment);
        
        if (CharacterManager.Instance != null)
        {
            if (!CharacterManager.Instance.SpendGold(enhanceCost))
            {
                return EnhancementResult.NotEnoughGold;
            }
        }
        else
        {
            return EnhancementResult.NotEnoughGold;
        }
        
        float successRate = GetSuccessRate(equipment.enhanceLevel);
        bool success = Random.value < successRate;
        
        if (success)
        {
            equipment.enhanceLevel++;
            
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.playerCharacter.CalculateTotalStats();
                CharacterManager.Instance.UpdateAllUI();
            }
            
            Debug.Log($"强化成功！{equipment.config.equipmentName} 现在是 +{equipment.enhanceLevel}");
            return EnhancementResult.Success;
        }
        else
        {
            Debug.Log($"强化失败，但 {equipment.config.equipmentName} 保持原样（保护卷轴）");
            return EnhancementResult.FailureNoPenalty;
        }
    }
}

public enum EnhancementResult
{
    Success,
    FailureNoPenalty,
    FailureDowngrade,
    Destroyed,
    MaxLevelReached,
    NotEnoughGold,
    InvalidEquipment
}
