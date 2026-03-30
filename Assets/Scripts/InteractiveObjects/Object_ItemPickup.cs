using UnityEngine;

public class Object_ItemPickup : MonoBehaviour
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        playerInventory = collision.GetComponent<Inventory_Base>();
        if (playerInventory != null && playerInventory.CanAddItem())
        {
            playerInventory.AddItem(itemToAdd);
            Destroy(gameObject);
        }
    }
}
