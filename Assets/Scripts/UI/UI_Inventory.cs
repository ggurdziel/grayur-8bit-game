using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    private UI_ItemSlot[] uiItemSlots;

    [SerializeField] private Inventory_Player inventory;

    private void Awake()
    {
        uiItemSlots = GetComponentsInChildren<UI_ItemSlot>();

        if (inventory == null)
        {
            Debug.LogError("UI_Inventory: Inventory_Player reference is missing.");
            return;
        }

        inventory.OnInventoryChange += UpdateInventorySlots;
        UpdateInventorySlots();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChange -= UpdateInventorySlots;
    }

    private void UpdateInventorySlots()
    {
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