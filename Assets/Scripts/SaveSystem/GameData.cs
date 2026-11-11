using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GameData
{
    public List<InventoryEntry> inventory = new List<InventoryEntry>();
    public string sceneName;
    public string lastSaved;
    public List<string> storyFlags = new List<string>();
}

[Serializable]
public class InventoryEntry
{
    public string saveID;
    public int stackSize;
    public int slotIndex = -1;
}
