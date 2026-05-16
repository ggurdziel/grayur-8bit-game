using UnityEngine;

public class UI_Menu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject inventorySection;
    [SerializeField] private GameObject settingsSection;

    private bool isOpen;

    private void Awake()
    {
        CloseMenu();
    }

    public void OpenMenu()
    {
        isOpen = true;

        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (inventorySection != null)
            inventorySection.SetActive(true);

        if (settingsSection != null)
            settingsSection.SetActive(true);
    }

    public void CloseMenu()
    {
        isOpen = false;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (inventorySection != null)
            inventorySection.SetActive(false);

        if (settingsSection != null)
            settingsSection.SetActive(false);
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}