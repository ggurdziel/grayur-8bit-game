using UnityEngine;

public class Inventory_NPC : Inventory_Base
{
    [SerializeField] private ItemType acceptedItemType;

    public bool CanAcceptItem(Inventory_Item item)
    {
        if (item == null || item.itemData == null)
            return false;

        return item.itemData.itemType == acceptedItemType;
    }
}