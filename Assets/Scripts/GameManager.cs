using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Start()
    {
        PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void PlaySceneMusic(string sceneName)
    {
        if (sceneName.StartsWith("House_"))
        {
            AudioManager.instance.PlayMusic("HouseTheme");
        }
        else if (sceneName == "Hanalei")
        {
            AudioManager.instance.PlayMusic("MainTheme");
        }
        else
        {
            AudioManager.instance.PlayMusic("HouseTheme");
        }
    }

    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        StartCoroutine(ChangeSceneCo(sceneName, respawnType));
    }

    private IEnumerator ChangeSceneCo(string sceneName, RespawnType respawnType)
    {
        if (ScreenFader.instance != null)
        {
            yield return ScreenFader.instance.FadeOut();
        }

        PlaySceneMusic(sceneName);

        SceneManager.LoadScene(sceneName);

        yield return null;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.LoadGame();
        }

        Vector3 position = GetWaypointPosition(respawnType);

        if (position != Vector3.zero && Player.instance != null)
        {
            Player.instance.transform.position = position;
        }

        if (ScreenFader.instance != null)
        {
            yield return ScreenFader.instance.FadeIn();
        }
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