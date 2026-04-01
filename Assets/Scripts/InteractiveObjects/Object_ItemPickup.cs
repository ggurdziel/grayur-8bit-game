using UnityEngine;

public class Object_ItemPickup : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    [SerializeField] private ItemDataSO itemData;

    private Inventory_Item itemToAdd;
    private Inventory_Base playerInventory;

    private void Awake()
    {
        itemToAdd = new Inventory_Item(itemData);
    }

    private void OnValidate()
    {
        if (itemData == null)
            return;
            
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = itemData.itemIcon;
        gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    public void Interact(Player player)
    {
        Inventory_Base inventory = player.GetComponent<Inventory_Base>();

        if (inventory != null && inventory.CanAddItem())
        {
            inventory.AddItem(itemToAdd);
            Destroy(gameObject);
        }
    }

    public void TryPickup()
    {
        if (playerInventory != null && playerInventory.CanAddItem())
        {
            playerInventory.AddItem(itemToAdd);
            Destroy(gameObject);
        }
    }
}