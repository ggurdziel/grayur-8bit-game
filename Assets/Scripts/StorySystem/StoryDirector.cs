using System.Collections.Generic;

public static class StoryDirector
{
    public static bool TryPlayFirst(
        IEnumerable<StoryBeat> beats,
        Player player,
        Object_NPC[] participants,
        Object_NPC npc = null)
    {
        var dm = UnityEngine.Object.FindFirstObjectByType<DialogueManager>(
            UnityEngine.FindObjectsInactive.Include);

        if (beats == null)
            return false;

        foreach (StoryBeat beat in beats)
        {
            if (beat == null) continue;

            if (beat.CanPlay(player, npc))
                return beat.Play(player, dm, participants);
        }

        return false;
    }
}