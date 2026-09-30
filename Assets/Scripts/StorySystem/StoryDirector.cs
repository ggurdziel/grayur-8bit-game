using System.Collections.Generic;

public static class StoryDirector
{
    public static bool TryPlayFirst(
        IEnumerable<StoryBeat> beats,
        Player player,
        Object_NPC[] participants)
    {
        var dm = UnityEngine.Object.FindFirstObjectByType<DialogueManager>();

        foreach (StoryBeat beat in beats)
        {
            if (beat == null) continue;

            bool ok = beat.CanPlay(player);
            UnityEngine.Debug.Log($"Beat '{beat.name}' can play: {ok}");

            if (ok)
                return beat.Play(player, dm, participants);
        }

        UnityEngine.Debug.Log("No beat matched, falling back to idle dialogue.");
        return false;
    }
}