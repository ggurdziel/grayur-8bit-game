using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveables;

    [SerializeField] private string fileName = "8-bit-game.json";
    [SerializeField] private bool encryptData = true;

    private void Awake() {
        instance = this;
    }

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
        if (dataHandler == null)
        {
            Debug.LogWarning("SaveManager: dataHandler is null, skipping save.");
            return;
        }

        if (allSaveables == null)
        {
            allSaveables = FindISaveables();
        }

        if (gameData == null)
        {
            gameData = new GameData();
        }

        foreach (ISaveable saveable in allSaveables)
        {
            if (saveable != null)
            {
                saveable.SaveData(ref gameData);
            }
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
