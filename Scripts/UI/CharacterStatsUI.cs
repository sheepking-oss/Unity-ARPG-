using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatsUI : MonoBehaviour
{
    [Header("基本信息")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI classText;
    public TextMeshProUGUI levelText;
    
    [Header("经验值")]
    public Slider experienceSlider;
    public TextMeshProUGUI experienceText;
    
    [Header("资源")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Slider manaSlider;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI goldText;
    
    [Header("基础属性")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI vitalityText;
    
    [Header("战斗属性")]
    public TextMeshProUGUI attackPowerText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI moveSpeedText;
    public TextMeshProUGUI criticalChanceText;
    public TextMeshProUGUI criticalDamageText;
    
    [Header("按钮")]
    public Button closeButton;
    public Button toggleButton;
    
    [Header("面板")]
    public GameObject statsPanel;
    
    private CharacterData currentData;
    private bool isVisible = true;
    
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
    
    public void UpdateStats(CharacterData data)
    {
        if (data == null) return;
        
        currentData = data;
        
        UpdateBasicInfo();
        UpdateExperience();
        UpdateResources();
        UpdateBaseStats();
        UpdateCombatStats();
    }
    
    private void UpdateBasicInfo()
    {
        if (characterNameText != null)
        {
            characterNameText.text = currentData.characterName;
        }
        
        if (classText != null && currentData.classConfig != null)
        {
            classText.text = currentData.classConfig.className;
        }
        
        if (levelText != null)
        {
            levelText.text = $"等级: {currentData.level}";
        }
    }
    
    private void UpdateExperience()
    {
        if (experienceSlider != null)
        {
            float experiencePercent = (float)currentData.experience / currentData.experienceToNextLevel;
            experienceSlider.value = Mathf.Clamp01(experiencePercent);
        }
        
        if (experienceText != null)
        {
            experienceText.text = $"{currentData.experience} / {currentData.experienceToNextLevel}";
        }
    }
    
    private void UpdateResources()
    {
        if (healthSlider != null)
        {
            float healthPercent = (float)currentData.currentHealth / currentData.totalStats.maxHealth;
            healthSlider.value = Mathf.Clamp01(healthPercent);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentData.currentHealth} / {currentData.totalStats.maxHealth}";
        }
        
        if (manaSlider != null)
        {
            float manaPercent = (float)currentData.currentMana / currentData.totalStats.maxMana;
            manaSlider.value = Mathf.Clamp01(manaPercent);
        }
        
        if (manaText != null)
        {
            manaText.text = $"{currentData.currentMana} / {currentData.totalStats.maxMana}";
        }
        
        if (goldText != null)
        {
            goldText.text = $"{currentData.gold}";
        }
    }
    
    private void UpdateBaseStats()
    {
        if (strengthText != null)
        {
            strengthText.text = $"{currentData.totalStats.strength}";
        }
        
        if (intelligenceText != null)
        {
            intelligenceText.text = $"{currentData.totalStats.intelligence}";
        }
        
        if (agilityText != null)
        {
            agilityText.text = $"{currentData.totalStats.agility}";
        }
        
        if (vitalityText != null)
        {
            vitalityText.text = $"{currentData.totalStats.vitality}";
        }
    }
    
    private void UpdateCombatStats()
    {
        if (attackPowerText != null)
        {
            attackPowerText.text = $"{currentData.totalStats.attackPower}";
        }
        
        if (defenseText != null)
        {
            defenseText.text = $"{currentData.totalStats.defense}";
        }
        
        if (attackSpeedText != null)
        {
            attackSpeedText.text = $"{currentData.totalStats.attackSpeed:F2}";
        }
        
        if (moveSpeedText != null)
        {
            moveSpeedText.text = $"{currentData.totalStats.moveSpeed:F2}";
        }
        
        if (criticalChanceText != null)
        {
            criticalChanceText.text = $"{currentData.totalStats.criticalChance * 100:F1}%";
        }
        
        if (criticalDamageText != null)
        {
            criticalDamageText.text = $"{currentData.totalStats.criticalDamage * 100:F1}%";
        }
    }
    
    public void Show()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            isVisible = true;
        }
    }
    
    public void Hide()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
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
