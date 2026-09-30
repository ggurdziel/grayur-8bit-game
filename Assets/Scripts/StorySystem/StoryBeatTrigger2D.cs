using UnityEngine;

public class StoryBeatTrigger2D : MonoBehaviour
{
    [SerializeField] private StoryBeat[] beats;
    [SerializeField] private Object_NPC[] participants;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        if (player == null) return;

        StoryDirector.TryPlayFirst(beats, player, participants);
    }
}