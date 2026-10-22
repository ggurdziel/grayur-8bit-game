using System.Collections.Generic;
using UnityEngine;

public class Object_NPCStory : Object_NPC
{
    [SerializeField] private StoryBeat[] beats;
    [SerializeField] private Object_NPC[] otherParticipants;

    [Header("Cutscene on interact (optional)")]
    [Tooltip("Plays once when the player talks to this NPC and the flags below are met.")]
    [SerializeField] private CutsceneSequence sequence;
    [SerializeField] private string[] sequenceRequiredFlags;
    [SerializeField] private string[] sequenceForbiddenFlags;

    public StoryBeat[] Beats => beats;

    // True while the sequence exists, hasn't played, and its flags are right.
    public bool SequenceAvailable
    {
        get
        {
            if (sequence == null || sequence.HasPlayed) return false;

            var flags = StoryFlags.Instance;
            if (flags == null) return false;

            if (sequenceRequiredFlags != null)
                foreach (var f in sequenceRequiredFlags)
                    if (!string.IsNullOrEmpty(f) && !flags.Has(f)) return false;

            if (sequenceForbiddenFlags != null)
                foreach (var f in sequenceForbiddenFlags)
                    if (!string.IsNullOrEmpty(f) && flags.Has(f)) return false;

            return true;
        }
    }

    public bool HasSequence => sequence != null;

    public override void Interact(Player player)
    {
        if (SequenceAvailable)
        {
            sequence.Play(player);
            return;
        }

        var participants = new List<Object_NPC> { this };

        if (otherParticipants != null)
            participants.AddRange(otherParticipants);

        if (beats != null && beats.Length > 0 &&
            StoryDirector.TryPlayFirst(beats, player, participants.ToArray(), this))
            return;

        base.Interact(player); // nothing matched: normal idle dialogue
    }
}