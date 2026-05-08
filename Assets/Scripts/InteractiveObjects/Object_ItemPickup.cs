using UnityEngine;

public class Object_ItemPickup : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;

    [SerializeField] private ItemDataSO itemData;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnValidate()
    {
        if (itemData == null)
            return;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = itemData.itemIcon;

        gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    public void Interact(Player player)
    {
        Inventory_Base inventory = player.GetComponent<Inventory_Base>();
        if (inventory == null || itemData == null)
            return;

        Inventory_Item itemToPickUp = new Inventory_Item(itemData);

        if (inventory.CanAddItem(itemToPickUp))
        {
            inventory.AddItem(itemToPickUp);
            Debug.Log("Picked up: " + itemData.itemName);
            Destroy(gameObject);
        }
    }
}