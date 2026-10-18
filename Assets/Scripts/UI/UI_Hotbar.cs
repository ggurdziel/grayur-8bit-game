using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Hotbar : MonoBehaviour
{
    private UI_ItemSlot[] uiItemSlots;
    private Inventory_Player inventory;

    private void Awake()
    {
        uiItemSlots = GetComponentsInChildren<UI_ItemSlot>(true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConnectToInventory();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Disconnect();
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m) => ConnectToInventory();

    private void Disconnect()
    {
        if (inventory != null)
            inventory.OnInventoryChange -= UpdateInventorySlots;
    }

    private void ConnectToInventory()
    {
        Disconnect();

        inventory = Player.instance != null
            ? Player.instance.GetComponent<Inventory_Player>()
            : FindFirstObjectByType<Inventory_Player>();

        if (inventory == null) return;

        inventory.OnInventoryChange += UpdateInventorySlots;
        UpdateInventorySlots();
    }

    private void UpdateInventorySlots()
    {
        if (inventory == null) return;

        for (int i = 0; i < uiItemSlots.Length; i++)
        {
            Debug.Log(
                "Hotbar slot " + i +
                " object=" + uiItemSlots[i].gameObject.name +
                " item=" + (inventory.GetItemAtIndex(i) == null ? "null" : inventory.GetItemAtIndex(i).itemData.itemName)
            );

            uiItemSlots[i].Setup(inventory, i);
            uiItemSlots[i].UpdateSlot(inventory.GetItemAtIndex(i));
        }
    }
}
