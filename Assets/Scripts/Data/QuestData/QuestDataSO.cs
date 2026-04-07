using UnityEngine;
using UnityEditor;

public enum RewardType { Item, None }
public enum QuestType { Kill, Talk, Delivery, Collect }

[CreateAssetMenu(menuName = "RPG Setup/Quest Data/New Quest", fileName = "Quest - ")]
public class QuestDataSO : ScriptableObject
{
    [Header("Quest Identity")]
    public string questSaveId;

    [Header("Quest Info")]
    public QuestType questType;
    public string questName;
    [TextArea] public string description;
    [TextArea] public string questGoal;

    [Header("Quest Objective")]
    public string questTargetId;
    public int requiredAmount = 1;
    public ItemDataSO itemToCollect;

    [Header("Reward Details")]
    public RewardType rewardType;
    public Inventory_Item[] rewardItems;

    private void OnValidate()
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            questSaveId = AssetDatabase.AssetPathToGUID(path);
        }
#endif
    }
}