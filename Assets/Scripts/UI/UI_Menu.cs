using UnityEngine;

public class UI_Menu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject inventorySection;
    [SerializeField] private GameObject settingsSection;
    [SerializeField] private GameObject saveSection;

    private bool isOpen;

    public void OpenMenu()
    {
        isOpen = true;

        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (inventorySection != null) inventorySection.SetActive(true);
        if (settingsSection != null)  settingsSection.SetActive(true);
        if (saveSection != null) saveSection.SetActive(false);
    }

    public void CloseMenu()
    {
        isOpen = false;

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void ShowInventorySection()
    {
        if (inventorySection != null)
            inventorySection.SetActive(true);

        if (settingsSection != null)
            settingsSection.SetActive(false);
    }

    public void ShowSettingsSection()
    {
        if (inventorySection != null)
            inventorySection.SetActive(false);

        if (settingsSection != null)
            settingsSection.SetActive(true);
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}