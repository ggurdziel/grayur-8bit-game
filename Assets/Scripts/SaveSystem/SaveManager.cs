using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public const int MaxSlots = 5;

    // Which save slot this play session reads and writes. -1 = none chosen (main menu).
    // Static so it survives scene changes even though SaveManager lives in each scene.
    public static int CurrentSlot { get; private set; } = -1;

    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveables;

    [SerializeField] private bool encryptData = true;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string FilePrefix = "8-bit-game-slot";
    private static bool encrypt = true;

    private bool IsMainMenu => gameObject.scene.name == mainMenuSceneName;

    private static string FileNameFor(int slot) => $"{FilePrefix}{slot}.json";

    private static FileDataHandler HandlerFor(int slot) =>
        new FileDataHandler(Application.persistentDataPath, FileNameFor(slot), encrypt);

    private void Awake()
    {
        instance = this;
        encrypt = encryptData;

        // Pressing Play directly in a game scene (skipping the main menu) uses slot 1,
        // so saving and loading still work while testing in the editor.
        if (CurrentSlot < 0 && !IsMainMenu)
            CurrentSlot = 0;
    }

    private IEnumerator Start()
    {
        allSaveables = FindISaveables();

        if (CurrentSlot < 0 || IsMainMenu)
            yield break; // main menu: nothing to load

        dataHandler = HandlerFor(CurrentSlot);

        yield return new WaitForSeconds(.01f);
        LoadGame();
    }

    // ---------------- Slot helpers (used by the main menu) ----------------

    // Reads a slot without loading it into the game. Null if the slot is empty.
    public static GameData PeekSlot(int slot)
    {
        try { return HandlerFor(slot).LoadData(); }
        catch (Exception e)
        {
            Debug.LogWarning($"Could not read save slot {slot + 1}: {e.Message}");
            return null;
        }
    }

    // Continue an existing save.
    public static void SelectSlot(int slot)
    {
        CurrentSlot = slot;
    }

    // Start fresh in a slot (wipes whatever was there).
    public static void StartNewSlot(int slot)
    {
        try { HandlerFor(slot).Delete(); }
        catch (Exception e) { Debug.LogWarning($"Could not clear slot {slot + 1}: {e.Message}"); }

        CurrentSlot = slot;
    }

    // Leave the current save (going back to the main menu).
    public static void ClearSlot()
    {
        CurrentSlot = -1;
    }

    // ---------------- Load / Save ----------------

    public void LoadGame()
    {
        if (CurrentSlot < 0 || IsMainMenu) return;

        if (dataHandler == null) dataHandler = HandlerFor(CurrentSlot);
        if (allSaveables == null) allSaveables = FindISaveables();

        gameData = dataHandler.LoadData();

        if (gameData == null)
        {
            Debug.Log($"No data in slot {CurrentSlot + 1}. Starting a new save.");
            gameData = new GameData();
            return;
        }

        foreach (var saveable in allSaveables)
        {
            if (saveable != null)
                saveable.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        // Never save from the main menu: it would overwrite the slot with an empty game.
        if (CurrentSlot < 0 || IsMainMenu) return;

        if (dataHandler == null) dataHandler = HandlerFor(CurrentSlot);
        if (allSaveables == null) allSaveables = FindISaveables();
        if (gameData == null) gameData = new GameData();

        foreach (ISaveable saveable in allSaveables)
        {
            if (saveable != null)
                saveable.SaveData(ref gameData);
        }

        // Shown on the main menu's save slots.
        gameData.sceneName = gameObject.scene.name;
        gameData.lastSaved = DateTime.Now.ToString("MMM d, yyyy  h:mm tt");

        dataHandler.SaveData(gameData);
    }

    // Save the current game into a chosen slot. It can differ from the slot the
    // game was loaded from (e.g. copy progress into a new slot, or overwrite another).
    public void SaveGameToSlot(int slot)
    {
        if (IsMainMenu || slot < 0 || slot >= MaxSlots) return;

        CurrentSlot = slot;
        dataHandler = HandlerFor(slot);
        SaveGame();
    }

    [ContextMenu("*** Delete ALL Save Slots ***")]
    public void DeleteSaveData()
    {
        encrypt = encryptData;
        for (int i = 0; i < MaxSlots; i++)
        {
            try { HandlerFor(i).Delete(); }
            catch { /* slot was already empty */ }
        }
    }

    [ContextMenu("Save Game Now")]
    public void DebugSaveGame()
    {
        SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<ISaveable> FindISaveables()
    {
        return
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .ToList();
    }

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            Debug.LogError("SaveManager instance is missing.");
            return;
        }

        instance.StartCoroutine(instance.LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // fade effect and sound effect

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sceneName);
    }
}