using UnityEngine;

public class Object_NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueLineSO firstDialogueLine;

    private UI ui;
    private Inventory_NPC npcInventory;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        npcInventory = GetComponent<Inventory_NPC>();
    }

    public void Interact(Player player)
    {
        Debug.Log("NPC interacted");

        if (player == null)
            return;

        Inventory_Player playerInventory = player.GetComponent<Inventory_Player>();

        if (playerInventory != null)
        {
            Inventory_Item selectedItem = playerInventory.GetSelectedItem();

            // Only the currently selected hotbar item can be given
            if (selectedItem != null && selectedItem.itemData != null)
            {
                if (npcInventory != null && npcInventory.CanAddItem())
                {
                    npcInventory.AddItem(selectedItem);
                    playerInventory.RemoveSelectedItem();

                    Debug.Log("Gave " + selectedItem.itemData.itemName + " to NPC.");
                    return;
                }
            }
        }

        // No selected item -> normal dialogue
        if (ui == null)
        {
            Debug.LogError("UI not found in scene.");
            return;
        }

        ui.OpenDialogueUI(firstDialogueLine);
    }
}