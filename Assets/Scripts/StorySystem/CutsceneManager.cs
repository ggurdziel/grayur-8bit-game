using UnityEngine;

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

        player = FindFirstObjectByType<Player>();
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
        if (player == null || player.input == null)
            return;

        if (enabled)
            player.input.Player.Enable();
        else
            player.input.Player.Disable();
    }
}