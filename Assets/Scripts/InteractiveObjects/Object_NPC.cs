using UnityEngine;

public class Object_NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueLineSO firstDialogueLine;
    private UI ui;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
    }

    public void Interact(Player player)
    {
        Debug.Log("NPC interacted");

        if (ui == null)
        {
            Debug.LogError("UI not found in scene.");
            return;
        }

        ui.OpenDialogueUI(firstDialogueLine);
    }
}