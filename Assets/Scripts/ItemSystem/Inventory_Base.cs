using System;
using System.Collections.Generic;
using UnityEngine;


public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChange;

    public int maxInventorySize = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    public bool CanAddItem() => itemList.Count < maxInventorySize;

    public void AddItem(Inventory_Item item)
    {
        itemList.Add(item);

        OnInventoryChange?.Invoke();
    }

}
