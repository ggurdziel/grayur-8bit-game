using UnityEngine;

public class Object_NPCQuestGiver : Object_NPC
{
    [SerializeField] private QuestDataSO questData;
    [SerializeField] private DialogueLineSO questStartLine;
    [SerializeField] private DialogueLineSO questInProgressLine;
    [SerializeField] private DialogueLineSO questCompleteLine;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Interact(Player player)
    {
        if (player == null || questData == null)
            return;

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager instance not found in scene.");
            return;
        }

        Inventory_Player playerInventory = player.GetComponent<Inventory_Player>();
        if (playerInventory == null)
        {
            Debug.LogError("Player does not have an Inventory_Player component.");
            return;
        }

        // Quest not started yet
        if (!QuestManager.Instance.HasQuest(questData))
        {
            QuestManager.Instance.StartQuest(questData);

            if (ui != null && questStartLine != null)
                ui.OpenDialogueUI(questStartLine);

            return;
        }

        // Already completed
        if (QuestManager.Instance.IsQuestCompleted(questData))
        {
            if (ui != null && questCompleteLine != null)
                ui.OpenDialogueUI(questCompleteLine);

            return;
        }

        // Quest active: check selected item only
        Inventory_Item selectedItem = playerInventory.GetSelectedItem();

        if (selectedItem != null &&
            selectedItem.itemData != null &&
            selectedItem.itemData == questData.itemToCollect)
        {
            playerInventory.RemoveSelectedItem();
            QuestManager.Instance.CompleteQuest(questData);

            Debug.Log("Quest item turned in: " + selectedItem.itemData.itemName);

            if (ui != null && questCompleteLine != null)
                ui.OpenDialogueUI(questCompleteLine);

            return;
        }

        // Player talked without the correct selected item
        if (ui != null && questInProgressLine != null)
            ui.OpenDialogueUI(questInProgressLine);
    }
}