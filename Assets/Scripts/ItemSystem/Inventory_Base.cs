using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChange;

    public int maxInventorySize = 13;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    [SerializeField] private int hotbarSize = 5;
    public int SelectedHotbarIndex { get; private set; } = 0;


    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        while (itemList.Count < maxInventorySize)
        {
            itemList.Add(null);
        }
    }


    public bool CanAddItem(Inventory_Item itemToAdd)
    {
        if (itemToAdd == null || itemToAdd.itemData == null)
            return false;

        return itemList.Contains(null);
    }

    public void AddItem(Inventory_Item item)
    {
        if (!CanAddItem(item))
            return;

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == null)
            {
                itemList[i] = item;
                NotifyInventoryChanged();
                return;
            }
        }
    }

    public void RemoveItem(Inventory_Item item)
    {
        if (item == null)
            return;

        int index = itemList.IndexOf(item);

        if (index >= 0)
        {
            itemList[index] = null;
            NotifyInventoryChanged();
        }
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

        itemList[SelectedHotbarIndex] = null;
        NotifyInventoryChanged();
    }

    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= hotbarSize)
            return;

        SelectedHotbarIndex = index;
        NotifyInventoryChanged();
    }

    public int GetHotbarSize()
    {
        return hotbarSize;
    }

    public Inventory_Item GetItemAtIndex(int index)
    {
        if (index < 0 || index >= itemList.Count)
            return null;

        return itemList[index];
    }

    public void SwapItems(int fromIndex, int toIndex)
    {
        Debug.Log("SwapItems called: " + fromIndex + " -> " + toIndex);

        if (fromIndex < 0 || toIndex < 0)
            return;

        if (fromIndex >= itemList.Count || toIndex >= itemList.Count)
        {
            Debug.LogWarning("Swap failed. itemList.Count = " + itemList.Count);
            return;
        }

        Inventory_Item temp = itemList[fromIndex];
        itemList[fromIndex] = itemList[toIndex];
        itemList[toIndex] = temp;

        Debug.Log(
            "After swap: index " + fromIndex + " = " +
            (itemList[fromIndex] == null ? "null" : itemList[fromIndex].itemData.itemName) +
            " | index " + toIndex + " = " +
            (itemList[toIndex] == null ? "null" : itemList[toIndex].itemData.itemName)
        );

        NotifyInventoryChanged();
    }

    protected void NotifyInventoryChanged()
    {
        OnInventoryChange?.Invoke();
    }
}