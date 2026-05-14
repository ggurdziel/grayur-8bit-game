using UnityEngine;

public class Object_Storage : MonoBehaviour, IInteractable
{
    private UI_Storage uiStorage;

    private void Start()
    {
        uiStorage = FindFirstObjectByType<UI_Storage>();
    }

    public void Interact(Player player)
    {
        if (uiStorage == null)
            return;

        uiStorage.ToggleStorage();
    }
}