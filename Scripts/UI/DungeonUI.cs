using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DungeonUI : MonoBehaviour
{
    [Header("副本信息面板")]
    public GameObject dungeonInfoPanel;
    public TextMeshProUGUI dungeonNameText;
    public TextMeshProUGUI dungeonDescriptionText;
    public TextMeshProUGUI recommendedLevelText;
    public TextMeshProUGUI difficultyText;
    
    [Header("进度显示")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI monstersLeftText;
    
    [Header("结果面板")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultMessageText;
    public TextMeshProUGUI resultRewardsText;
    public Button resultButton;
    
    [Header("导航按钮")]
    public Button closeButton;
    public Button returnButton;
    
    [Header("面板")]
    public GameObject dungeonPanel;
    
    private bool isVisible = false;
    private DungeonConfig currentDungeon;
    
    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnButtonClick);
        }
        
        if (resultButton != null)
        {
            resultButton.onClick.AddListener(OnResultButtonClick);
        }
        
        Hide();
    }
    
    public void UpdateDungeonInfo(DungeonConfig dungeon)
    {
        if (dungeon == null) return;
        
        currentDungeon = dungeon;
        
        if (dungeonNameText != null)
        {
            dungeonNameText.text = dungeon.dungeonName;
        }
        
        if (dungeonDescriptionText != null)
        {
            dungeonDescriptionText.text = dungeon.description;
        }
        
        if (recommendedLevelText != null)
        {
            recommendedLevelText.text = $"推荐等级: {dungeon.recommendedLevel}";
        }
        
        if (difficultyText != null)
        {
            difficultyText.text = GetDifficultyText(dungeon.difficulty);
            difficultyText.color = GetDifficultyColor(dungeon.difficulty);
        }
        
        if (deathsText != null)
        {
            deathsText.text = $"死亡: 0/{dungeon.maxDeaths}";
        }
    }
    
    public void UpdateTimer(float currentTime, float maxTime)
    {
        if (timerText == null) return;
        
        float remainingTime = maxTime - currentTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        
        timerText.text = $"时间: {minutes:D2}:{seconds:D2}";
        
        if (remainingTime < 60f)
        {
            timerText.color = Color.red;
        }
        else if (remainingTime < 120f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }
    }
    
    public void UpdateDeaths(int currentDeaths, int maxDeaths)
    {
        if (deathsText == null) return;
        
        deathsText.text = $"死亡: {currentDeaths}/{maxDeaths}";
        
        if (currentDeaths >= maxDeaths - 1)
        {
            deathsText.color = Color.red;
        }
        else
        {
            deathsText.color = Color.white;
        }
    }
    
    public void UpdateMonstersLeft(int monstersLeft)
    {
        if (monstersLeftText == null) return;
        
        monstersLeftText.text = $"剩余怪物: {monstersLeft}";
    }
    
    public void ShowCompletion(DungeonConfig dungeon)
    {
        if (resultPanel == null) return;
        
        if (resultTitleText != null)
        {
            resultTitleText.text = "副本完成！";
            resultTitleText.color = Color.green;
        }
        
        if (resultMessageText != null)
        {
            resultMessageText.text = $"恭喜你完成了 {dungeon.dungeonName}！";
        }
        
        if (resultRewardsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("获得奖励:");
            sb.AppendLine($"  经验值: {dungeon.completionExperience}");
            sb.AppendLine($"  金币: {dungeon.completionGold}");
            resultRewardsText.text = sb.ToString();
        }
        
        if (resultButton != null)
        {
            resultButton.GetComponentInChildren<TextMeshProUGUI>().text = "返回主城";
        }
        
        resultPanel.SetActive(true);
    }
    
    public void ShowFailure(string reason)
    {
        if (resultPanel == null) return;
        
        if (resultTitleText != null)
        {
            resultTitleText.text = "副本失败！";
            resultTitleText.color = Color.red;
        }
        
        if (resultMessageText != null)
        {
            resultMessageText.text = reason;
        }
        
        if (resultRewardsText != null)
        {
            resultRewardsText.text = "未获得任何奖励";
        }
        
        if (resultButton != null)
        {
            resultButton.GetComponentInChildren<TextMeshProUGUI>().text = "返回主城";
        }
        
        resultPanel.SetActive(true);
    }
    
    private void OnReturnButtonClick()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ReturnToMainCity();
        }
    }
    
    private void OnResultButtonClick()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ReturnToMainCity();
        }
    }
    
    private string GetDifficultyText(DungeonDifficulty difficulty)
    {
        switch (difficulty)
        {
            case DungeonDifficulty.Normal:
                return "普通";
            case DungeonDifficulty.Hard:
                return "困难";
            case DungeonDifficulty.Nightmare:
                return "噩梦";
            case DungeonDifficulty.Hell:
                return "地狱";
            default:
                return "";
        }
    }
    
    private Color GetDifficultyColor(DungeonDifficulty difficulty)
    {
        switch (difficulty)
        {
            case DungeonDifficulty.Normal:
                return Color.white;
            case DungeonDifficulty.Hard:
                return Color.yellow;
            case DungeonDifficulty.Nightmare:
                return new Color(0.7f, 0.2f, 0.9f);
            case DungeonDifficulty.Hell:
                return Color.red;
            default:
                return Color.white;
        }
    }
    
    public void Show()
    {
        if (dungeonPanel != null)
        {
            dungeonPanel.SetActive(true);
            isVisible = true;
        }
    }
    
    public void Hide()
    {
        if (dungeonPanel != null)
        {
            dungeonPanel.SetActive(false);
            isVisible = false;
        }
        
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
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
