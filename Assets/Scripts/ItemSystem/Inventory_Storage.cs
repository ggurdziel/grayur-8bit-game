using UnityEngine;

public class Inventory_Storage : Inventory_Base
{
    private Inventory_Player playerInventory;

    public void SetInventory(Inventory_Player inventory)
    {
        playerInventory = inventory;
    }

    public void FromPlayerToStorage(Inventory_Item item)
    {
        if (playerInventory == null || item == null)
            return;

        if (!CanAddItem(item))
            return;

        playerInventory.RemoveItem(item);
        AddItem(item);
    }

    public void FromStorageToPlayer(Inventory_Item item)
    {
        if (playerInventory == null || item == null)
            return;

        if (!playerInventory.CanAddItem(item))
            return;

        RemoveItem(item);
        playerInventory.AddItem(item);
    }
}