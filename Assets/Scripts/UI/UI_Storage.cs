using System.Collections.Generic;
using UnityEngine;

public class UI_Storage : MonoBehaviour
{
    private UI_ItemSlot[] uiItemSlots;
    private Inventory_Player inventory;

    private void Awake()
    {
        uiItemSlots = GetComponentsInChildren<UI_ItemSlot>(true);
    }

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory_Player>();

        if (inventory != null)
        {
            inventory.OnInventoryChange += UpdateInventoryUI;
            UpdateInventoryUI();
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChange -= UpdateInventoryUI;
        }
    }

    public void ToggleStorage()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void UpdateInventoryUI()
    {
        if (inventory == null) 
            return;

        int startIndex = inventory.GetHotbarSize();

        for (int i = 0; i < uiItemSlots.Length; i++)
        {
            int inventoryIndex = startIndex + i;

            Debug.Log(
                "Inventory UI slot " + i +
                " object=" + uiItemSlots[i].gameObject.name +
                " inventoryIndex=" + inventoryIndex +
                " item=" + (inventory.GetItemAtIndex(inventoryIndex) == null ? "null" : inventory.GetItemAtIndex(inventoryIndex).itemData.itemName)
            );

            uiItemSlots[i].Setup(inventory, inventoryIndex);
            uiItemSlots[i].UpdateSlot(inventory.GetItemAtIndex(inventoryIndex));
        }
    }
}

