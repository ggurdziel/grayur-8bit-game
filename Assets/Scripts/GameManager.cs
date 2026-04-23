using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void PlaySceneMusic(string sceneName)
{
    switch (sceneName)
    {
        case "House_1":
        case "House_2":
            AudioManager.instance.PlayMusic("HouseTheme");
            break;

        case "Hanalei":
            AudioManager.instance.PlayMusic("MainTheme");
            break;

        default:
            AudioManager.instance.PlayMusic("MainTheme");
            break;
    }
}

    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        StartCoroutine(ChangeSceneCo(sceneName, respawnType));
    }

    private IEnumerator ChangeSceneCo(string sceneName, RespawnType respawnType)
    {
        PlaySceneMusic(sceneName);

        SceneManager.LoadScene(sceneName);
        
        yield return null; // wait one frame

        Vector3 position = GetWaypointPosition(respawnType);

        if (position != Vector3.zero && Player.instance != null)
        {
            Player.instance.transform.position = position;
        }

        SaveManager.instance.LoadGame();
    }

    private Vector3 GetWaypointPosition(RespawnType type)
    {
        var waypoints = FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None);

        foreach (var point in waypoints)
        {
            if (point.GetWaypointType() == type)
            {
                point.SetCanBeTriggered(false);
                return point.GetPosition();
            }
        }
        return Vector3.zero;
    }
}