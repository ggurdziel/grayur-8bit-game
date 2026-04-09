using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveables;

    [SerializeField] private string fileName = "8-bit-game.json";
    [SerializeField] private bool encryptData = true;

    private IEnumerator Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        allSaveables = FindISaveables();

        yield return new WaitForSeconds(.01f);
        LoadGame();
    }


    public void LoadGame()
    {
        gameData = dataHandler.LoadData();

        if (gameData == null)
        {
            Debug.Log("No data found. Creating new save.");
            gameData = new GameData();
            return;
        }

        foreach (var saveable in allSaveables)
        {
            saveable.LoadData(gameData);
        }
    }


    public void SaveGame()
    {
        foreach (ISaveable saveable in allSaveables)
        {
            saveable.SaveData(ref gameData);
        }

        dataHandler.SaveData(gameData);
    }


    [ContextMenu("*** Delete Save Data ***")]
    public void DeleteSaveData()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        dataHandler.Delete();
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
}
