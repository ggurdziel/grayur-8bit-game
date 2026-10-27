using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

// The 5-slot list, shared by the main menu and the in-game save screen.
// Works like dialogue choices: one marker on the selected slot, W/S or arrows
// to move, Space to confirm. Put this on SaveList.
public class UI_SaveSlotList : MonoBehaviour
{
    [SerializeField] private Button[] slotButtons = new Button[SaveManager.MaxSlots];        // SaveSlot1..5
    [SerializeField] private GameObject[] slotMarkers = new GameObject[SaveManager.MaxSlots]; // hat icon on each slot
    [SerializeField] private string emptySlotText = "Empty";

    public event Action<int> SelectionChanged;
    public event Action Confirmed; // Space pressed

    public int Selected { get; private set; }
    public int SlotCount => slotButtons.Length;
    public GameData SelectedData => GetData(Selected);
    public bool InputEnabled { get; set; } = true;

    private GameData[] slotData;
    private bool initialized;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (initialized) return;
        initialized = true;

        slotData = new GameData[slotButtons.Length];

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i; // copy for the lambda
            slotButtons[i].onClick.AddListener(() => { if (InputEnabled) Select(index); });

            // Stop Unity's built-in UI navigation from also reacting to arrows/Space.
            var nav = slotButtons[i].navigation;
            nav.mode = Navigation.Mode.None;
            slotButtons[i].navigation = nav;
        }
    }

    private void Update()
    {
        if (!InputEnabled) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            Select((Selected - 1 + slotButtons.Length) % slotButtons.Length);

        if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
            Select((Selected + 1) % slotButtons.Length);

        if (kb.spaceKey.wasPressedThisFrame)
            Confirmed?.Invoke();
    }

    // Re-reads every slot from disk and updates the labels.
    public void Refresh()
    {
        Init();

        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotData[i] = SaveManager.PeekSlot(i);

            TMP_Text label = slotButtons[i].GetComponentInChildren<TMP_Text>();
            if (label == null) continue;

            GameData save = slotData[i];
            if (save == null)
                label.text = emptySlotText;
            else if (string.IsNullOrEmpty(save.sceneName))
                label.text = "Saved Game";
            else
                label.text = $"{save.sceneName}  -  {save.lastSaved}";
        }
    }

    public GameData GetData(int slot)
    {
        Init();
        return slot >= 0 && slot < slotData.Length ? slotData[slot] : null;
    }

    public int FirstEmptySlot()
    {
        for (int i = 0; i < slotButtons.Length; i++)
            if (GetData(i) == null) return i;
        return -1;
    }

    public void Select(int index)
    {
        Selected = Mathf.Clamp(index, 0, slotButtons.Length - 1);

        for (int i = 0; i < slotMarkers.Length; i++)
            if (slotMarkers[i] != null)
                slotMarkers[i].SetActive(i == Selected);

        ClearFocus();
        SelectionChanged?.Invoke(Selected);
    }

    // Clear UI focus so Space doesn't also "click" whatever button was last clicked.
    public static void ClearFocus()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}