using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // True while a scene transition is running (fade out -> load -> fade in).
    public static bool IsChangingScene { get; private set; }

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
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusicForScene(sceneName);
        }
    }

    // In-game transitions (doors, paths): put the player at a waypoint.
    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        StartCoroutine(ChangeSceneCo(sceneName, respawnType));
    }

    // Menu transitions (new game, load game, save & quit): no waypoint.
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneCo(sceneName, null));
    }

    private IEnumerator ChangeSceneCo(string sceneName, RespawnType? respawnType)
    {
        if (IsChangingScene)
        {
            Debug.LogWarning($"GameManager: ignored ChangeScene(\"{sceneName}\") because a scene change is already running.");
            yield break;
        }
        IsChangingScene = true;

        if (ScreenFader.instance != null)
        {
            yield return ScreenFader.instance.FadeOut();
        }

        // Keep progress from the scene we're leaving (skipped automatically in the main menu).
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveGame();
        }

        PlaySceneMusic(sceneName);

        SceneManager.LoadScene(sceneName);

        yield return null;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.LoadGame();
        }

        if (respawnType.HasValue)
        {
            Vector3 position = GetWaypointPosition(respawnType.Value);

            if (position != Vector3.zero && Player.instance != null)
            {
                Player.instance.transform.position = position;
            }
        }

        // The new scene is in place. Clear the flag before the fade-in, so a fade
        // that never finishes can't block every scene change after it.
        IsChangingScene = false;

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