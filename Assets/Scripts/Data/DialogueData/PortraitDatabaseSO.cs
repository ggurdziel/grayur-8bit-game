using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Dialogue Data/Portrait Database", fileName = "PORTRAIT DATABASE")]
public class PortraitDatabaseSO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Exactly as the speaker's name appears in dialogue, e.g. Marie, Max, Arnold Mille")]
        public string speakerName;
        public Sprite portrait;
    }

    public Entry[] entries;

    public Sprite Get(string speakerName)
    {
        if (string.IsNullOrEmpty(speakerName) || entries == null)
            return null;

        foreach (var e in entries)
            if (string.Equals(e.speakerName.Trim(), speakerName.Trim(), StringComparison.OrdinalIgnoreCase))
                return e.portrait;

        return null;
    }
}