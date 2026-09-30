using UnityEngine;

public class StoryBeatOnStart : MonoBehaviour
{
    [SerializeField] private StoryBeat[] beats;
    [SerializeField] private Object_NPC[] participants;

    private System.Collections.IEnumerator Start()
    {
        yield return null; 
        var player = FindFirstObjectByType<Player>();
        StoryDirector.TryPlayFirst(beats, player, participants);
    }
}