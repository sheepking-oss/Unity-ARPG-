using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterHealthBar : MonoBehaviour
{
    [Header("UI元素")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI monsterNameText;
    public GameObject healthBarObject;
    public Canvas canvas;
    
    [Header("Boss设置")]
    public bool isBossBar = false;
    public GameObject bossPhaseIndicator;
    public TextMeshProUGUI phaseText;
    
    [Header("设置")]
    public Vector3 offset = new Vector3(0, 2, 0);
    public float hideDelay = 3f;
    
    private Transform target;
    private float hideTimer = 0f;
    private int maxHealth = 100;
    private int currentHealth = 100;
    
    public void Initialize(int health, int maxHealth, string name, bool isBoss = false)
    {
        this.currentHealth = health;
        this.maxHealth = maxHealth;
        isBossBar = isBoss;
        
        if (monsterNameText != null)
        {
            monsterNameText.text = name;
        }
        
        UpdateHealth(health, maxHealth);
        
        if (isBossBar && bossPhaseIndicator != null)
        {
            bossPhaseIndicator.SetActive(true);
        }
    }
    
    public void UpdateHealth(int health, int maxHealth)
    {
        currentHealth = health;
        this.maxHealth = maxHealth;
        
        if (healthSlider != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthSlider.value = Mathf.Clamp01(healthPercent);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        
        Show();
        hideTimer = hideDelay;
    }
    
    public void ShowDamageNumber(int damage)
    {
        DamageNumberUI damageNumber = GetComponentInChildren<DamageNumberUI>();
        if (damageNumber != null)
        {
            damageNumber.ShowDamage(damage, false);
        }
    }
    
    public void UpdatePhase(int phase, string phaseName)
    {
        if (phaseText != null)
        {
            phaseText.text = phaseName;
        }
    }
    
    public void Show()
    {
        if (healthBarObject != null)
        {
            healthBarObject.SetActive(true);
        }
    }
    
    public void Hide()
    {
        if (healthBarObject != null)
        {
            healthBarObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (target != null && healthBarObject != null && healthBarObject.activeSelf)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + offset);
            
            RectTransform rectTransform = healthBarObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = screenPosition;
            }
        }
        
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0 && !isBossBar)
            {
                Hide();
            }
        }
    }
    
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
}
