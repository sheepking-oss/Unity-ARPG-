using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumberUI : MonoBehaviour
{
    [Header("UI元素")]
    public TextMeshProUGUI damageText;
    public Canvas canvas;
    
    [Header("设置")]
    public float floatSpeed = 1f;
    public float floatHeight = 1f;
    public float displayTime = 1f;
    public float fadeOutTime = 0.5f;
    
    [Header("颜色")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.yellow;
    public Color healColor = Color.green;
    
    private Vector3 startPosition;
    private Coroutine displayCoroutine;
    
    public void ShowDamage(int damage, bool isCritical)
    {
        if (damageText == null) return;
        
        damageText.text = damage.ToString();
        damageText.color = isCritical ? criticalDamageColor : normalDamageColor;
        
        if (isCritical)
        {
            damageText.fontSize = 36;
            damageText.fontStyle = FontStyles.Bold;
        }
        else
        {
            damageText.fontSize = 24;
            damageText.fontStyle = FontStyles.Normal;
        }
        
        gameObject.SetActive(true);
        
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        startPosition = transform.position;
        displayCoroutine = StartCoroutine(DisplayDamage());
    }
    
    public void ShowHeal(int amount)
    {
        if (damageText == null) return;
        
        damageText.text = $"+{amount}";
        damageText.color = healColor;
        damageText.fontSize = 28;
        damageText.fontStyle = FontStyles.Normal;
        
        gameObject.SetActive(true);
        
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        startPosition = transform.position;
        displayCoroutine = StartCoroutine(DisplayDamage());
    }
    
    private IEnumerator DisplayDamage()
    {
        float elapsedTime = 0f;
        float startY = startPosition.y;
        float targetY = startY + floatHeight;
        
        while (elapsedTime < displayTime)
        {
            elapsedTime += Time.deltaTime;
            
            float progress = elapsedTime / displayTime;
            float newY = Mathf.Lerp(startY, targetY, progress);
            
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            if (elapsedTime > displayTime - fadeOutTime)
            {
                float fadeProgress = (elapsedTime - (displayTime - fadeOutTime)) / fadeOutTime;
                damageText.alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }
            
            yield return null;
        }
        
        damageText.alpha = 1f;
        gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
    }
}
