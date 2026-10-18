using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Inventory : MonoBehaviour
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

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StartCoroutine(ReconnectNextFrame());
    }

    // Waits a frame so the new scene's Player has finished Awake.
    private System.Collections.IEnumerator ReconnectNextFrame()
    {
        yield return null;
        ConnectToInventory();
    }

    private void Disconnect()
    {
        if (inventory != null)
            inventory.OnInventoryChange -= UpdateInventoryUI;
    }

    private void ConnectToInventory()
    {
        Disconnect();

        inventory = Player.instance != null
            ? Player.instance.GetComponent<Inventory_Player>()
            : FindFirstObjectByType<Inventory_Player>();

        if (inventory == null)
            return;

        inventory.OnInventoryChange += UpdateInventoryUI;
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        if (inventory == null)
            return;

        int startIndex = inventory.GetHotbarSize();

        for (int i = 0; i < uiItemSlots.Length; i++)
        {
            int inventoryIndex = startIndex + i;
            Inventory_Item item = inventory.GetItemAtIndex(inventoryIndex);

            uiItemSlots[i].Setup(inventory, inventoryIndex);
            uiItemSlots[i].UpdateSlot(item);
        }
    }
}