using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private Dictionary<string, Quest> questLookup = new Dictionary<string, Quest>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void StartQuest(QuestDataSO questData)
    {
        if (questData == null) return;

        if (!questLookup.ContainsKey(questData.questSaveId))
        {
            Quest newQuest = new Quest(questData);
            newQuest.isStarted = true;
            questLookup.Add(questData.questSaveId, newQuest);

            Debug.Log($"Started quest: {questData.questName}");
        }
    }


    public bool HasQuest(QuestDataSO questData)
    {
        return questData != null && questLookup.ContainsKey(questData.questSaveId);
    }


    public bool IsQuestCompleted(QuestDataSO questData)
    {
        if (questData == null) return false;
        if (!questLookup.ContainsKey(questData.questSaveId)) return false;

        return questLookup[questData.questSaveId].isCompleted;
    }


    public Quest GetQuest(QuestDataSO questData)
    {
        if (questData == null) return null;
        if (!questLookup.ContainsKey(questData.questSaveId)) return null;

        return questLookup[questData.questSaveId];
    }


    public void CompleteQuest(QuestDataSO questData)
    {
        if (questData == null) return;
        if (!questLookup.ContainsKey(questData.questSaveId)) return;

        Quest quest = questLookup[questData.questSaveId];

        if (quest.isCompleted) return;

        quest.isCompleted = true;
        Debug.Log($"Completed quest: {questData.questName}");

        GiveRewards(questData);
    }


    private void GiveRewards(QuestDataSO questData)
    {
        if (questData.rewardType == RewardType.None) return;

        Player player = FindFirstObjectByType<Player>();
        if (player == null) return;

        Inventory_Base inventory = player.GetComponent<Inventory_Base>();
        if (inventory == null) return;

        if (questData.rewardItems == null) return;

        foreach (Inventory_Item reward in questData.rewardItems)
        {
            if (reward != null)
            {
                inventory.AddItem(reward);
            }
        }
    }
}