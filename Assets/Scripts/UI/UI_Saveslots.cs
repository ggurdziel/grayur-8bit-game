using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Everything that gets written to a save file.
// Add your own progress fields here (quest state, inventory, story flags, etc.).
// JsonUtility can save: int, float, bool, string, arrays/Lists of those, and [Serializable] classes.
[Serializable]
public class SaveData
{
    public int slot;
    public string sceneName;
    public float playerX;
    public float playerY;
    public string savedAt;
}

// Static helper: no GameObject needed. Each save slot is a JSON file in
// Application.persistentDataPath/saves/slot_N.json
public static class SaveSlots
{
    // The slot the current play session saves into (-1 = none yet).
    public static int CurrentSlot { get; private set; } = -1;

    // Save waiting to be applied once its scene finishes loading.
    private static SaveData pendingLoad;

    private static string Folder => Path.Combine(Application.persistentDataPath, "saves");
    private static string PathFor(int slot) => Path.Combine(Folder, $"slot_{slot}.json");

    public static bool HasSaves =>
        Directory.Exists(Folder) && Directory.GetFiles(Folder, "slot_*.json").Length > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ---------- Called from the main menu ----------

    public const int MaxSlots = 5;

    // Returns the save in a slot, or null if the slot is empty.
    public static SaveData GetSave(int slot) => Read(slot);

    // Starts a fresh game that will save into `slot` (overwrites it on the next save).
    public static void StartNewGame(string firstScene, int slot)
    {
        CurrentSlot = slot;
        pendingLoad = null;
        SceneManager.LoadScene(firstScene);
    }

    // First empty slot, or -1 if all are full.
    public static int FirstEmptySlot()
    {
        for (int i = 0; i < MaxSlots; i++)
            if (!File.Exists(PathFor(i))) return i;
        return -1;
    }

    public static void LoadGame(int slot)
    {
        SaveData data = Read(slot);
        if (data == null) return;

        CurrentSlot = slot;
        pendingLoad = data;
        SceneManager.LoadScene(data.sceneName);
    }

    public static List<SaveData> GetAllSaves()
    {
        var saves = new List<SaveData>();
        if (!Directory.Exists(Folder)) return saves;

        foreach (string file in Directory.GetFiles(Folder, "slot_*.json"))
        {
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(file));
                if (data != null) saves.Add(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Skipping unreadable save {file}: {e.Message}");
            }
        }

        saves.Sort((a, b) => a.slot.CompareTo(b.slot));
        return saves;
    }

    // ---------- Called from the game (Save & Quit) ----------

    public static void SaveCurrent()
    {
        if (CurrentSlot < 0) CurrentSlot = NextFreeSlot();

        var data = new SaveData
        {
            slot = CurrentSlot,
            sceneName = SceneManager.GetActiveScene().name,
            savedAt = DateTime.Now.ToString("MMM d, yyyy  h:mm tt")
        };

        Player player = UnityEngine.Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            Vector3 p = player.transform.position;
            data.playerX = p.x;
            data.playerY = p.y;
        }

        // TODO: copy your quest / inventory / story progress into `data` here.

        Directory.CreateDirectory(Folder);
        File.WriteAllText(PathFor(CurrentSlot), JsonUtility.ToJson(data, true));
        Debug.Log($"Saved slot {CurrentSlot} to {PathFor(CurrentSlot)}");
    }

    // ---------- Internals ----------

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoad == null || scene.name != pendingLoad.sceneName) return;

        Player player = UnityEngine.Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.transform.position = new Vector3(
                pendingLoad.playerX, pendingLoad.playerY, player.transform.position.z);
        }

        // TODO: restore your quest / inventory / story progress from `pendingLoad` here.

        pendingLoad = null;
    }

    private static SaveData Read(int slot)
    {
        string path = PathFor(slot);
        if (!File.Exists(path)) return null;

        try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
        catch { return null; }
    }

    private static int NextFreeSlot()
    {
        int free = FirstEmptySlot();
        return free >= 0 ? free : 0;
    }
}