using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryFlags : MonoBehaviour
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
        }
    }

    // For saving/loading later
    public IReadOnlyCollection<string> All => flags;

    public void Load(IEnumerable<string> saved)
    {
        flags.Clear();
        foreach (string f in saved)
            flags.Add(f);
    }
}