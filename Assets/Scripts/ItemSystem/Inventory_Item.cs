using System;
using UnityEngine;

[Serializable]
public class Inventory_Item
{

    private string itemId;
    public int stackSize = 1;

    public ItemDataSO itemData;

    public Inventory_Item(ItemDataSO itemData)
    {
        this.itemData = itemData;
        itemId = itemData.itemName + "_" + Guid.NewGuid();
    }

}
