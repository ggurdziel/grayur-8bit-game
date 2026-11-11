using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private Dictionary<string, Quest> questLookup = new Dictionary<string, Quest>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            new GameObject("QuestManager").AddComponent<QuestManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Returns the quest, rebuilding it from the saved story flags after a reload.
    private Quest Find(QuestDataSO data)
    {
        if (data == null) return null;

        if (questLookup.TryGetValue(data.questSaveId, out Quest quest)) return quest;

        var flags = StoryFlags.Instance;
        if (flags != null && flags.Has("quest." + data.name + ".started"))
        {
            quest = new Quest(data);
            quest.isStarted = true;
            quest.isCompleted = flags.Has("quest." + data.name + ".done");
            questLookup.Add(data.questSaveId, quest);
            return quest;
        }

        return null;
    }

    public void StartQuest(QuestDataSO questData)
    {
        if (questData == null) return;

        if (Find(questData) == null)
        {
            Quest newQuest = new Quest(questData);
            newQuest.isStarted = true;
            questLookup.Add(questData.questSaveId, newQuest);

            Debug.Log($"Started quest: {questData.questName}");
        }

        StoryFlags.Instance.Set("quest." + questData.name + ".started");
    }


    public bool HasQuest(QuestDataSO questData) => Find(questData) != null;


    public bool IsQuestCompleted(QuestDataSO questData)
    {
        Quest quest = Find(questData);
        return quest != null && quest.isCompleted;
    }


    public Quest GetQuest(QuestDataSO questData) => Find(questData);


    public void CompleteQuest(QuestDataSO questData)
    {
        Quest quest = Find(questData);
        if (quest == null || quest.isCompleted) return;

        quest.isCompleted = true;
        Debug.Log($"Completed quest: {questData.questName}");

        StoryFlags.Instance.Set("quest." + questData.name + ".done");

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
                inventory.GiveItem(reward);
            }
        }
    }

    // True if an active quest wants this item.
    public bool IsQuestItem(ItemDataSO data, out QuestDataSO quest)
    {
        foreach (var q in questLookup.Values)
        {
            if (q.isCompleted || q.questData.requiredItems == null) continue;

            if (System.Array.IndexOf(q.questData.requiredItems, data) >= 0)
            {
                quest = q.questData;
                return true;
            }
        }

        quest = null;
        return false;
    }

}