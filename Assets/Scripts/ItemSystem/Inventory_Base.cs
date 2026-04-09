using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChange;

    public int maxInventorySize = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    [SerializeField] private int hotbarSize = 5;
    public int SelectedHotbarIndex { get; private set; } = 0;

    public bool CanAddItem() => itemList.Count < maxInventorySize;

    public void AddItem(Inventory_Item item)
    {
        if (!CanAddItem())
            return;

        itemList.Add(item);
        Debug.Log("Added item to inventory: " + item.itemData.itemName + " | Count: " + itemList.Count);
        OnInventoryChange?.Invoke();
    }

    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= hotbarSize)
            return;

        SelectedHotbarIndex = index;
        OnInventoryChange?.Invoke();
    }

    public Inventory_Item GetSelectedItem()
    {
        if (SelectedHotbarIndex < 0 || SelectedHotbarIndex >= itemList.Count)
            return null;

        Debug.Log("Selected slot " + SelectedHotbarIndex + " contains: " + itemList[SelectedHotbarIndex].itemData.itemName);
        return itemList[SelectedHotbarIndex];
    }

    public void RemoveSelectedItem()
    {
        if (SelectedHotbarIndex < 0 || SelectedHotbarIndex >= itemList.Count)
            return;

        Debug.Log("Removed item from slot " + SelectedHotbarIndex + ": " + itemList[SelectedHotbarIndex].itemData.itemName);
        itemList.RemoveAt(SelectedHotbarIndex);

        if (SelectedHotbarIndex >= itemList.Count && SelectedHotbarIndex > 0)
            SelectedHotbarIndex--;

        OnInventoryChange?.Invoke();
    }

    public int GetHotbarSize()
    {
        return hotbarSize;
    }


    protected void NotifyInventoryChanged()
    {
        OnInventoryChange?.Invoke();
    }
    
}