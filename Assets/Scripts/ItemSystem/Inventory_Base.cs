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

        return itemList[SelectedHotbarIndex];
    }

    public void RemoveSelectedItem()
    {
        if (SelectedHotbarIndex < 0 || SelectedHotbarIndex >= itemList.Count)
            return;

        itemList.RemoveAt(SelectedHotbarIndex);

        if (SelectedHotbarIndex >= itemList.Count && SelectedHotbarIndex > 0)
            SelectedHotbarIndex--;

        OnInventoryChange?.Invoke();
    }

    public int GetHotbarSize()
    {
        return hotbarSize;
    }
}