using UnityEngine;

public class Object_NPCStory : Object_NPC
{
    [SerializeField] private StoryBeat[] beats;
    [SerializeField] private Object_NPC[] otherParticipants;

    public override void Interact(Player player)
    {
        Debug.Log($"[{name}] Object_NPCStory.Interact, beats: {(beats == null ? 0 : beats.Length)}");
        var participants = new System.Collections.Generic.List<Object_NPC> { this };
        participants.AddRange(otherParticipants);

        if (StoryDirector.TryPlayFirst(beats, player, participants.ToArray()))
            return;

        base.Interact(player); // no beat matched: normal idle dialogue
    }
}