using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    public bool IsInCutscene { get; private set; }

    private Player player;
    private Object_NPC[] cutsceneNPCs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // A scene change mid-cutscene would otherwise leave the player locked
    // and every later beat refusing to play.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = null; // re-find in the new scene

        if (IsInCutscene)
        {
            cutsceneNPCs = null;
            IsInCutscene = false;
            SetPlayerControls(true);
        }
    }

    private Player GetPlayer()
    {
        if (player == null)
            player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();

        return player;
    }

    public void StartCutscene(Object_NPC[] npcs)
    {
        if (IsInCutscene)
            return;

        IsInCutscene = true;
        cutsceneNPCs = npcs;

        SetPlayerControls(false);

        if (cutsceneNPCs != null)
        {
            foreach (Object_NPC npc in cutsceneNPCs)
            {
                if (npc != null)
                    npc.EnterCutscene();
            }
        }
    }

    public void EndCutscene()
    {
        if (!IsInCutscene)
            return;

        if (cutsceneNPCs != null)
        {
            foreach (Object_NPC npc in cutsceneNPCs)
            {
                if (npc != null)
                    npc.ExitCutscene();
            }
        }

        cutsceneNPCs = null;
        IsInCutscene = false;

        SetPlayerControls(true);
    }

    private void SetPlayerControls(bool enabled)
    {
        var p = GetPlayer();
        if (p == null || p.input == null)
            return;

        if (enabled)
            p.input.Player.Enable();
        else
            p.input.Player.Disable();
    }
}