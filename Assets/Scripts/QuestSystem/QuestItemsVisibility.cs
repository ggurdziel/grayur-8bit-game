using UnityEngine;

public class QuestItemsVisibility : MonoBehaviour
{
    [SerializeField] private QuestDataSO quest;

    [Tooltip("Hidden once the player holds every required item, and stays hidden after the quest is done.")]
    [SerializeField] private GameObject[] hideWhenReady;

    [Tooltip("Shown once the player holds every required item, until the quest is done.")]
    [SerializeField] private GameObject[] showWhenReady;

    private Inventory_Base inventory;

    private void Update()
    {
        var qm = QuestManager.Instance;
        if (quest == null || qm == null || !qm.HasQuest(quest)) return;   // scene defaults apply

        bool completed = qm.IsQuestCompleted(quest);
        bool ready = completed || HoldsAllItems();
        if (!ready) return;

        SetAll(hideWhenReady, false);
        SetAll(showWhenReady, !completed);
    }

    private bool HoldsAllItems()
    {
        if (inventory == null && Player.instance != null)
            inventory = Player.instance.GetComponent<Inventory_Base>();

        return inventory != null &&
               quest.requiredItems != null && quest.requiredItems.Length > 0 &&
               inventory.HasItems(quest.requiredItems);
    }

    private static void SetAll(GameObject[] list, bool active)
    {
        if (list == null) return;

        foreach (var go in list)
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
    }
}