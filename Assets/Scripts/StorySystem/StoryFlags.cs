using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryFlags : MonoBehaviour, ISaveable
{
    public static StoryFlags Instance { get; private set; }

    private readonly HashSet<string> flags = new HashSet<string>();

    public event Action<string> FlagSet;

    // Creates itself, so you can start play mode from any scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("StoryFlags").AddComponent<StoryFlags>();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string flag) => flags.Contains(flag);

    public void Set(string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return;

        if (flags.Add(flag))
        {
            Debug.Log("Story flag set: " + flag);
            FlagSet?.Invoke(flag);

            // Autosave so a passed flag can't be redone.
            if (SaveManager.instance != null) SaveManager.instance.RequestSave();
        }
    }

    public IReadOnlyCollection<string> All => flags;

    public void SaveData(ref GameData data)
    {
        data.storyFlags = new List<string>(flags);
    }

    // Fills the set directly (not through Set) so loading doesn't log, fire events or save.
    public void LoadData(GameData data)
    {
        flags.Clear();
        if (data.storyFlags != null)
            foreach (string f in data.storyFlags) flags.Add(f);
    }
}