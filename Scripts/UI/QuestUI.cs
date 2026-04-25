using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    [Header("任务列表")]
    public List<QuestItemUI> activeQuestSlots;
    public List<QuestItemUI> completedQuestSlots;
    
    [Header("任务详情")]
    public GameObject questDetailPanel;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questRequirementsText;
    public TextMeshProUGUI questRewardsText;
    public Button turnInButton;
    
    [Header("导航按钮")]
    public Button closeButton;
    public Button toggleButton;
    public Button activeTabButton;
    public Button completedTabButton;
    
    [Header("面板")]
    public GameObject questPanel;
    public GameObject activeQuestsContent;
    public GameObject completedQuestsContent;
    
    private QuestInstance selectedQuest;
    private bool isVisible = false;
    private bool showingActiveQuests = true;
    
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
        
        if (activeTabButton != null)
        {
            activeTabButton.onClick.AddListener(ShowActiveQuests);
        }
        
        if (completedTabButton != null)
        {
            completedTabButton.onClick.AddListener(ShowCompletedQuests);
        }
        
        if (turnInButton != null)
        {
            turnInButton.onClick.AddListener(OnTurnInButtonClick);
        }
        
        Hide();
        ShowActiveQuests();
    }
    
    public void UpdateQuestList()
    {
        UpdateActiveQuestList();
        UpdateCompletedQuestList();
    }
    
    private void UpdateActiveQuestList()
    {
        if (QuestManager.Instance == null) return;
        
        List<QuestInstance> activeQuests = QuestManager.Instance.GetActiveQuests();
        
        for (int i = 0; i < activeQuestSlots.Count; i++)
        {
            if (i < activeQuests.Count)
            {
                activeQuestSlots[i].SetQuest(activeQuests[i]);
                activeQuestSlots[i].gameObject.SetActive(true);
            }
            else
            {
                activeQuestSlots[i].ClearQuest();
                activeQuestSlots[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateCompletedQuestList()
    {
        if (QuestManager.Instance == null) return;
        
        List<QuestInstance> completedQuests = QuestManager.Instance.GetCompletedQuests();
        
        for (int i = 0; i < completedQuestSlots.Count; i++)
        {
            if (i < completedQuests.Count)
            {
                completedQuestSlots[i].SetQuest(completedQuests[i]);
                completedQuestSlots[i].gameObject.SetActive(true);
            }
            else
            {
                completedQuestSlots[i].ClearQuest();
                completedQuestSlots[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void OnQuestSelect(QuestItemUI itemUI)
    {
        if (itemUI == null || itemUI.questInstance == null) return;
        
        selectedQuest = itemUI.questInstance;
        ShowQuestDetails(selectedQuest);
    }
    
    private void ShowQuestDetails(QuestInstance questInstance)
    {
        if (questDetailPanel == null) return;
        
        QuestConfig config = questInstance.config;
        
        if (questNameText != null)
        {
            questNameText.text = config.questName;
        }
        
        if (questDescriptionText != null)
        {
            questDescriptionText.text = config.description;
        }
        
        if (questRequirementsText != null)
        {
            questRequirementsText.text = GetRequirementsText(questInstance);
        }
        
        if (questRewardsText != null)
        {
            questRewardsText.text = GetRewardsText(config);
        }
        
        if (turnInButton != null)
        {
            turnInButton.interactable = questInstance.status == QuestStatus.Completed;
        }
        
        questDetailPanel.SetActive(true);
    }
    
    private string GetRequirementsText(QuestInstance questInstance)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("任务要求:");
        
        foreach (var requirement in questInstance.config.requirements)
        {
            string status = requirement.currentAmount >= requirement.requiredAmount ? "[完成]" : "[进行中]";
            sb.AppendLine($"  {requirement.targetName} ({requirement.currentAmount}/{requirement.requiredAmount}) {status}");
        }
        
        return sb.ToString();
    }
    
    private string GetRewardsText(QuestConfig config)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("任务奖励:");
        
        if (config.experienceReward > 0)
        {
            sb.AppendLine($"  经验值: {config.experienceReward}");
        }
        
        if (config.goldReward > 0)
        {
            sb.AppendLine($"  金币: {config.goldReward}");
        }
        
        foreach (var equipmentReward in config.equipmentRewards)
        {
            EquipmentConfig equipment = EquipmentManager.Instance?.GetEquipmentById(equipmentReward.equipmentId);
            if (equipment != null)
            {
                sb.AppendLine($"  装备: {equipment.equipmentName}");
            }
        }
        
        return sb.ToString();
    }
    
    private void OnTurnInButtonClick()
    {
        if (selectedQuest == null) return;
        
        if (QuestManager.Instance != null)
        {
            bool success = QuestManager.Instance.TurnInQuest(selectedQuest);
            if (success)
            {
                selectedQuest = null;
                questDetailPanel.SetActive(false);
                UpdateQuestList();
            }
        }
    }
    
    private void ShowActiveQuests()
    {
        showingActiveQuests = true;
        
        if (activeQuestsContent != null)
        {
            activeQuestsContent.SetActive(true);
        }
        
        if (completedQuestsContent != null)
        {
            completedQuestsContent.SetActive(false);
        }
        
        if (activeTabButton != null)
        {
            activeTabButton.interactable = false;
        }
        
        if (completedTabButton != null)
        {
            completedTabButton.interactable = true;
        }
    }
    
    private void ShowCompletedQuests()
    {
        showingActiveQuests = false;
        
        if (activeQuestsContent != null)
        {
            activeQuestsContent.SetActive(false);
        }
        
        if (completedQuestsContent != null)
        {
            completedQuestsContent.SetActive(true);
        }
        
        if (activeTabButton != null)
        {
            activeTabButton.interactable = true;
        }
        
        if (completedTabButton != null)
        {
            completedTabButton.interactable = false;
        }
    }
    
    public void Show()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
            isVisible = true;
        }
        
        UpdateQuestList();
    }
    
    public void Hide()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
            isVisible = false;
        }
        
        if (questDetailPanel != null)
        {
            questDetailPanel.SetActive(false);
        }
        
        selectedQuest = null;
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

public class QuestItemUI : MonoBehaviour
{
    [Header("UI元素")]
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questStatusText;
    public Button button;
    public Image selectionHighlight;
    
    [Header("状态")]
    public QuestInstance questInstance;
    public bool isSelected = false;
    
    private QuestUI parentQuestUI;
    
    private void Awake()
    {
        parentQuestUI = GetComponentInParent<QuestUI>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void SetQuest(QuestInstance quest)
    {
        questInstance = quest;
        
        if (questNameText != null)
        {
            questNameText.text = quest.config.questName;
        }
        
        if (questStatusText != null)
        {
            switch (quest.status)
            {
                case QuestStatus.InProgress:
                    questStatusText.text = "进行中";
                    questStatusText.color = Color.yellow;
                    break;
                case QuestStatus.Completed:
                    questStatusText.text = "已完成";
                    questStatusText.color = Color.green;
                    break;
                case QuestStatus.TurnedIn:
                    questStatusText.text = "已提交";
                    questStatusText.color = Color.gray;
                    break;
            }
        }
    }
    
    public void ClearQuest()
    {
        questInstance = null;
        
        if (questNameText != null)
        {
            questNameText.text = "";
        }
        
        if (questStatusText != null)
        {
            questStatusText.text = "";
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
        if (parentQuestUI != null && questInstance != null)
        {
            parentQuestUI.OnQuestSelect(this);
        }
    }
}
