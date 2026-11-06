using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base, ISaveable
{
    [SerializeField] private ItemListDataSO itemListData;

    public bool TryGiveSelectedItem(Inventory_Base targetInventory)
    {
        if (targetInventory == null)
            return false;

        Inventory_Item selectedItem = GetSelectedItem();

        if (selectedItem == null || selectedItem.itemData == null)
            return false;

        if (!targetInventory.CanAddItem(selectedItem))
            return false;

        targetInventory.AddItem(selectedItem);
        RemoveSelectedItem();

        return true;
    }


    public void SaveData(ref GameData data)
    {
        data.inventory.Clear();

        for (int i = 0; i < itemList.Count; i++)
        {
            Inventory_Item item = itemList[i];
            if (item == null || item.itemData == null)
                continue;

            data.inventory.Add(new InventoryEntry
            {
                saveID = item.itemData.saveID,
                stackSize = item.stackSize,
                slotIndex = i
            });
        }
    }

    public void LoadData(GameData data)
    {
        itemList.Clear();
        while (itemList.Count < maxInventorySize)
            itemList.Add(null);

        foreach (var savedItem in data.inventory)
        {
            ItemDataSO itemData = itemListData.GetItemData(savedItem.saveID);
            if (itemData == null)
            {
                Debug.Log("Item not found: " + savedItem.saveID);
                continue;
            }

            Inventory_Item itemToLoad = new Inventory_Item(itemData);
            itemToLoad.stackSize = savedItem.stackSize;

            int index = savedItem.slotIndex;

            if (index < 0 || index >= itemList.Count || itemList[index] != null)
                index = itemList.IndexOf(null);

            if (index == -1)
                break; 

            itemList[index] = itemToLoad;
        }

        NotifyInventoryChanged();
    }
}