using Unity.Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Articy.Unity;
using UnityEngine;

public enum CutsceneStepType { Dialogue, FadeOut, FadeIn, Wait, MoveActor, SetActive, SetFlag, SnapBlack, StartQuest, WaitForBackground, GiveItem, PanCamera, ResetCamera }

public enum MoveDir { Left, Right, Up, Down }

public enum ActiveMode { Show, Hide, Destroy }

[Serializable]
public class MoveLeg
{
    public MoveDir direction;
    [Tooltip("World units. 1 is roughly one tile if your tiles are 1 unit.")]
    public float distance = 1f;
}

[Serializable]
public class CutsceneStep
{
    public string label;
    public CutsceneStepType type;

    [Header("Timing")]
    [Tooltip("Start this step and immediately go on to the next one, without waiting for it to finish. Never tick this on WaitForBackground.")]
    public bool runWithNext;

    [Header("Dialogue (use Lines OR Flow Start/End)")]
    public ArticyRef[] lines;
    public ArticyRef flowStart;
    public ArticyRef flowEnd;

    [Tooltip("The flow counts as successful if it passed through ANY of these lines.")]
    public ArticyRef[] successNodes;
    [Tooltip("Replay the flow until a success node is reached.")]
    public bool repeatUntilSuccess;
    [Tooltip("Optional shorter flow to replay on retries. Empty = replay Flow Start.")]
    public ArticyRef retryStart;
    public ArticyRef retryEnd;

    [Header("Wait")]
    public float seconds = 1f;

    [Header("Fade (FadeOut / FadeIn / SnapBlack)")]
    [Tooltip("Optional. FadeOut and SnapBlack show this picture over the black.")]
    public Sprite fadePicture;
    [Tooltip("0 = use the ScreenFader's default speed.")]
    public float fadeSeconds = 0f;

    [Header("MoveActor (leave Npc empty to move the player)")]
    public Object_NPC npc;
    [Tooltip("Walked one after another. If empty, Destination is used instead.")]
    public List<MoveLeg> path = new List<MoveLeg>();
    public Transform destination;
    public float speed = 2f;
    [Tooltip("Remove the NPC from the scene once the walk finishes (e.g. Max leaving).")]
    public bool destroyAfterMove;

    [Header("SetActive")]
    public GameObject target;
    public ActiveMode activeMode = ActiveMode.Hide;
    [HideInInspector] public bool active = true;

    [Header("SetFlag")]
    public string flag;

    [Header("StartQuest")]
    public QuestDataSO quest;
    
    [Header("GiveItem")]
    public Inventory_Item[] items;

    [Header("PanCamera")]
    public Transform cameraDestination;
    public float cameraSeconds = 2f;
}

public class CutsceneSequence : MonoBehaviour
{
    [Tooltip("NPCs that freeze during the cutscene. Leave out an NPC that walks away in the background, so he isn't released early.")]
    [SerializeField] private Object_NPC[] cutsceneNpcs;
    [SerializeField] private List<CutsceneStep> steps = new List<CutsceneStep>();

    [Header("Auto start")]
    [SerializeField] private bool playOnStart;
    [Tooltip("Plays only if this flag is NOT set yet; the flag is set when it begins.")]
    [SerializeField] private string oncePlayedFlag;

    [Header("Control")]
    [Tooltip("Give the player control back once the last non-background step finishes, even if a background step (like a walk) is still running.")]
    [SerializeField] private bool releasePlayerBeforeBackgroundEnds = true;

    private bool running;
    private int activeSteps;

    private string Key =>
        !string.IsNullOrEmpty(oncePlayedFlag)
            ? oncePlayedFlag
            : "cutscene." + gameObject.scene.name + "." + gameObject.name;

    public bool HasPlayed =>
        StoryFlags.Instance != null && StoryFlags.Instance.Has(Key);

    private IEnumerator Start()
    {
        if (!playOnStart) yield break;
        if (HasPlayed) yield break;

        Debug.Log($"[Cutscene] Start: fader={(ScreenFader.instance != null ? "OK" : "NULL")}");
        if (ScreenFader.instance != null) ScreenFader.instance.SetBlack();
        yield return null;

        Play(FindFirstObjectByType<Player>());
    }

    public void Play(Player player)
    {
        if (running || HasPlayed) return;

        if (StoryFlags.Instance != null)
            StoryFlags.Instance.Set(Key);

        StartCoroutine(Run(player));
    }

    private CinemachineCamera vcam;
    private Transform cameraProxy;
    private Transform originalFollow;

    private IEnumerator PanCamera(CutsceneStep s)
    {
        if (s.cameraDestination == null) yield break;

        if (vcam == null) vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam == null) { Debug.LogWarning("[Cutscene] No CinemachineCamera found"); yield break; }

        if (cameraProxy == null)
        {
            originalFollow = vcam.Follow;
            cameraProxy = new GameObject("CutsceneCameraProxy").transform;
            cameraProxy.position = originalFollow != null ? originalFollow.position : vcam.transform.position;
            vcam.Follow = cameraProxy;
        }

        Transform proxy = cameraProxy;
        Vector3 from = proxy.position;
        Vector3 to = s.cameraDestination.position;
        to.z = from.z;

        float duration = Mathf.Max(0.01f, s.cameraSeconds);
        float t = 0f;
        while (t < 1f)
        {
            if (proxy == null) yield break;
            t += Time.deltaTime / duration;
            proxy.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        if (proxy != null) proxy.position = to;
    }

    private void ResetCamera()
    {
        if (vcam != null && cameraProxy != null)
        {
            vcam.Follow = originalFollow;
            Destroy(cameraProxy.gameObject);
            cameraProxy = null;
        }
    }

    private IEnumerator Run(Player player)
    {
        running = true;
        activeSteps = 0;

        if (CutsceneManager.Instance != null)
            CutsceneManager.Instance.StartCutscene(cutsceneNpcs);

        foreach (var step in steps)
        {
            if (step.runWithNext)
                StartCoroutine(RunTracked(step, player));
            else
                yield return RunStep(step, player);
        }

        // Optionally keep the player locked until background steps finish.
        if (!releasePlayerBeforeBackgroundEnds)
            yield return new WaitUntil(() => activeSteps == 0);

        if (CutsceneManager.Instance != null)
            CutsceneManager.Instance.EndCutscene();

        running = false;

        // Leftover background steps (e.g. Max still walking) finish after the player is free.
        yield return new WaitUntil(() => activeSteps == 0);
    }

    private IEnumerator RunTracked(CutsceneStep step, Player player)
    {
        activeSteps++;
        yield return RunStep(step, player);
        activeSteps--;
    }

    private IEnumerator RunStep(CutsceneStep s, Player player)
    {
        switch (s.type)
        {
            case CutsceneStepType.Dialogue:
                yield return PlayDialogue(s);
                break;

            case CutsceneStepType.FadeOut:
                Debug.Log($"[Cutscene] FadeOut step picture={(s.fadePicture != null ? s.fadePicture.name : "NULL")}");
                if (ScreenFader.instance != null)
                    yield return ScreenFader.instance.FadeOut(s.fadePicture, s.fadeSeconds);
                break;

            case CutsceneStepType.FadeIn:
                if (ScreenFader.instance != null)
                    yield return ScreenFader.instance.FadeIn(s.fadeSeconds);
                break;

            case CutsceneStepType.SnapBlack:
                if (ScreenFader.instance != null)
                    ScreenFader.instance.SetBlack(s.fadePicture);
                break;

            case CutsceneStepType.Wait:
                yield return new WaitForSeconds(s.seconds);
                break;

            case CutsceneStepType.MoveActor:
                yield return RunMove(s, player);
                break;

            case CutsceneStepType.SetActive:
                if (s.target != null)
                {
                    switch (s.activeMode)
                    {
                        case ActiveMode.Show:    s.target.SetActive(true);  break;
                        case ActiveMode.Hide:    s.target.SetActive(false); break;
                        case ActiveMode.Destroy: Destroy(s.target);         break;
                    }
                }
                break;

            case CutsceneStepType.SetFlag:
                if (StoryFlags.Instance != null) StoryFlags.Instance.Set(s.flag);
                break;

            case CutsceneStepType.StartQuest:
                if (s.quest != null && QuestManager.Instance != null &&
                    !QuestManager.Instance.HasQuest(s.quest))
                    QuestManager.Instance.StartQuest(s.quest);
                break;

            case CutsceneStepType.GiveItem:
                if (s.items != null && player != null)
                {
                    var inv = player.GetComponent<Inventory_Base>();
                    if (inv != null)
                        foreach (var item in s.items)
                            if (item != null && item.itemData != null)
                                inv.AddItem(item);
                }
                break;

            case CutsceneStepType.WaitForBackground:
                yield return new WaitUntil(() => activeSteps == 0);
                break;

            case CutsceneStepType.PanCamera:
                yield return PanCamera(s);
                break;

            case CutsceneStepType.ResetCamera:
                ResetCamera();
                break;
        }
    }

    // ------------------------------------------------------------
    // Movement
    // ------------------------------------------------------------

    private IEnumerator RunMove(CutsceneStep s, Player player)
    {
        Transform actor = s.npc != null
            ? s.npc.transform
            : (player != null ? player.transform : null);

        if (actor == null) yield break;

        if (s.path != null && s.path.Count > 0)
        {
            foreach (var leg in s.path)
            {
                if (actor == null) yield break;   // destroyed mid-walk

                Vector2 target = (Vector2)actor.position + DirToVector(leg.direction) * leg.distance;

                if (s.npc != null) yield return s.npc.WalkTo(target, s.speed);
                else yield return MovePlayer(player, target, s.speed);
            }
        }
        else if (s.destination != null)
        {
            if (s.npc != null) yield return s.npc.WalkTo(s.destination.position, s.speed);
            else yield return MovePlayer(player, s.destination.position, s.speed);
        }

        if (s.destroyAfterMove && s.npc != null)
            Destroy(s.npc.gameObject);
    }

    private static Vector2 DirToVector(MoveDir d)
    {
        switch (d)
        {
            case MoveDir.Left:  return Vector2.left;
            case MoveDir.Right: return Vector2.right;
            case MoveDir.Up:    return Vector2.up;
            default:            return Vector2.down;
        }
    }

    private IEnumerator MovePlayer(Player player, Vector3 target, float speed)
    {
        if (player == null) yield break;

        while (Vector2.Distance(player.transform.position, target) > 0.05f)
        {
            Vector2 pos = player.transform.position;
            Vector2 dir = ((Vector2)target - pos).normalized;

            player.UpdateAnimation(dir);
            player.transform.position = Vector2.MoveTowards(pos, target, speed * Time.deltaTime);
            yield return null;
        }

        player.UpdateAnimation(Vector2.zero);
    }

    // ------------------------------------------------------------
    // Dialogue
    // ------------------------------------------------------------

    private IEnumerator PlayDialogue(CutsceneStep s)
    {
        var dm = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
        if (dm == null) yield break;

        if (!s.flowStart.HasReference)
        {
            if (s.lines == null) yield break;

            var list = new List<ArticyObject>();
            foreach (var r in s.lines)
                if (r.HasReference) list.Add(r.GetObject());

            if (list.Count == 0) yield break;

            yield return RunAndWait(dm, () => dm.StartSequence(list));
            yield break;
        }

        bool hasSuccessNodes = false;
        if (s.successNodes != null)
            foreach (var r in s.successNodes)
                if (r.HasReference) { hasSuccessNodes = true; break; }

        int attempt = 0;

        while (true)
        {
            bool retry = attempt > 0 && s.retryStart.HasReference;

            ArticyObject start = retry ? s.retryStart.GetObject() : s.flowStart.GetObject();
            ArticyObject end = retry
                ? (s.retryEnd.HasReference ? s.retryEnd.GetObject() : null)
                : (s.flowEnd.HasReference ? s.flowEnd.GetObject() : null);

            yield return RunAndWait(dm, () => dm.StartDialogue(start, end));

            if (!hasSuccessNodes || !s.repeatUntilSuccess)
                yield break;

            if (dm.HasVisitedAny(s.successNodes))
                yield break;

            attempt++;
            yield return null;
        }
    }

    private IEnumerator RunAndWait(DialogueManager dm, Action start)
    {
        bool done = false;
        Action handler = null;
        handler = () => { dm.DialogueEnded -= handler; done = true; };

        dm.DialogueEnded += handler;
        start();

        yield return new WaitUntil(() => done);
    }
}