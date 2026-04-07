using UnityEngine;

public class Inventory_Player : Inventory_Base
{
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
}