using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public void Interact(Player player)
    {
        Debug.Log("Talking to NPC");

        // later:
        // open dialogue UI
        // check held item
        // handle gifting
    }
}