using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FileDataHandler
{
    private string fullPath;
    private bool encryptData;
    private string codeWord = "yourcode";

    public FileDataHandler(string dataDirPath, string dataFileName, bool encryptData)
    {
        fullPath = Path.Combine(dataDirPath, dataFileName);
        this.encryptData = encryptData;
    }

    public void SaveData(GameData gameData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToSave = JsonUtility.ToJson(gameData, true);

            if (encryptData)
            {
                dataToSave = EncryptDecrypt(dataToSave);
            }

            File.WriteAllText(fullPath, dataToSave);
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving data: " + e);
        }
    }

    public GameData LoadData()
    {
        if (!File.Exists(fullPath))
            return null;

        try
        {
            string dataToLoad = File.ReadAllText(fullPath);

            if (encryptData)
            {
                dataToLoad = EncryptDecrypt(dataToLoad);
            }

            return JsonUtility.FromJson<GameData>(dataToLoad);
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading data: " + e);
            return null;
        }
    }

    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";

        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ codeWord[i % codeWord.Length]);
        }

        return modifiedData;
    }

    public void Delete()
    {
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}