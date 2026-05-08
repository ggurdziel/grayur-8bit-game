using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, 
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler, 
    IDropHandler,
    IPointerEnterHandler,
    IPointerClickHandler
{
    public Inventory_Item itemInSlot { get; private set; }

    [Header("UI Slot Setup")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemStackSize;

    private Inventory_Player inventory;
    private int slotIndex;
    private Canvas canvas;

    private static UI_ItemSlot draggedSlot;
    private static GameObject dragIcon;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(Inventory_Player inventory, int slotIndex)
    {
        this.inventory = inventory;
        this.slotIndex = slotIndex;
    }

    public void UpdateSlot(Inventory_Item item)
    {
        itemInSlot = item;

        if (itemIcon == null)
        {
            Debug.LogError("UI_ItemSlot missing Item Icon reference on: " + gameObject.name);
            return;
        }

        if (itemStackSize != null)
            itemStackSize.text = "";

        if (itemInSlot == null || itemInSlot.itemData == null)
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
            return;
        }

        itemIcon.enabled = true;
        itemIcon.sprite = itemInSlot.itemData.itemIcon;
        itemIcon.color = Color.white;

        if (itemStackSize != null && itemInSlot.stackSize > 1)
        {
            itemStackSize.text = itemInSlot.stackSize.ToString();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin drag on: " + gameObject.name);

        if (inventory == null)
        {
            Debug.LogWarning("No inventory assigned to slot: " + gameObject.name);
            return;
        }

        if (itemInSlot == null)
        {
            Debug.LogWarning("No item in slot: " + gameObject.name);
            return;
        }

        if (itemInSlot.itemData == null)
        {
            Debug.LogWarning("Item has no itemData in slot: " + gameObject.name);
            return;
        }

        Debug.Log("Dragging item: " + itemInSlot.itemData.itemName + " from index " + slotIndex);

        draggedSlot = this;

        dragIcon = new GameObject("Dragging Item Icon");
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        Image image = dragIcon.AddComponent<Image>();
        image.sprite = itemInSlot.itemData.itemIcon;
        image.raycastTarget = false;
        image.color = Color.white;

        CanvasGroup canvasGroup = dragIcon.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        RectTransform rect = dragIcon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64, 64);
        rect.position = eventData.position;

        itemIcon.color = new Color(1, 1, 1, 0.35f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag on: " + gameObject.name);

        if (dragIcon != null)
            Destroy(dragIcon);

        if (itemIcon != null && itemInSlot != null)
        {
            itemIcon.color = new Color(1, 1, 1, 0.9f);
        }

        StartCoroutine(ClearDraggedSlotNextFrame());
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop called on: " + gameObject.name);

        if (draggedSlot == null)
            return;

        if (draggedSlot == this)
            return;

        if (inventory == null)
            return;

        Debug.Log("Dropped from " + draggedSlot.slotIndex + " to " + slotIndex);
        inventory.SwapItems(draggedSlot.slotIndex, slotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hovering over slot: " + gameObject.name);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked slot: " + gameObject.name);
    }

    private System.Collections.IEnumerator ClearDraggedSlotNextFrame()
    {
        yield return null;
        draggedSlot = null;
    }
}