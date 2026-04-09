using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GameData
{
    public List<InventoryEntry> inventory = new List<InventoryEntry>();
}

[Serializable]
public class InventoryEntry
{
    public string saveID;
    public int stackSize;
}
