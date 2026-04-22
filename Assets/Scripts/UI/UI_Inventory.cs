using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    private UI_ItemSlot[] uiItemSlots;
    private Inventory_Player inventory;

    private void Awake()
    {
        uiItemSlots = GetComponentsInChildren<UI_ItemSlot>(true);
    }

    private void Start()
    {
        ConnectToInventory();
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChange -= UpdateInventorySlots;
        }
    }

    private void ConnectToInventory()
    {
        inventory = FindFirstObjectByType<Inventory_Player>();

        if (inventory == null)
        {
            Debug.LogError("UI_Inventory: Inventory_Player reference is missing.");
            return;
        }

        inventory.OnInventoryChange -= UpdateInventorySlots;
        inventory.OnInventoryChange += UpdateInventorySlots;

        UpdateInventorySlots();
    }

    private void UpdateInventorySlots()
    {
        if (inventory == null) return;

        List<Inventory_Item> itemList = inventory.itemList;

        for (int i = 0; i < uiItemSlots.Length; i++)
        {
            if (i < itemList.Count)
                uiItemSlots[i].UpdateSlot(itemList[i]);
            else
                uiItemSlots[i].UpdateSlot(null);
        }
    }
}