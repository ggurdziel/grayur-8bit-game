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

        if (!targetInventory.CanAddItem())
            return false;

        targetInventory.AddItem(selectedItem);
        RemoveSelectedItem();

        return true;
    }


    public void SaveData(ref GameData data)
    {
        data.inventory.Clear();

        foreach (var item in itemList)
        {
            if (item != null && item.itemData != null)
            {
                string saveID = item.itemData.saveID;

                InventoryEntry existing = data.inventory
                    .Find(x => x.saveID == saveID);

                if (existing != null)
                {
                    existing.stackSize += item.stackSize;
                }
                else
                {
                    data.inventory.Add(new InventoryEntry
                    {
                        saveID = saveID,
                        stackSize = item.stackSize
                    });
                }
            }
        }
    }


    public void LoadData(GameData data)
    {
        itemList.Clear();
        foreach (var item in data.inventory)
        {
            string saveId = item.saveID;
            int stackSize = item.stackSize;

            ItemDataSO itemData = itemListData.GetItemData(saveId);

            if (itemData == null)
            {
                Debug.Log("Item not found: " + saveId);
                continue;
            }

            Inventory_Item itemToLoad = new Inventory_Item(itemData);
            itemToLoad.stackSize = stackSize;

            itemList.Add(itemToLoad);
        }

        NotifyInventoryChanged();
    }
}