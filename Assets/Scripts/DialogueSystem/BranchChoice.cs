using Articy.Unity;
using Articy.Unity.Interfaces;
using TMPro;
using UnityEngine;

public class BranchChoice : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;

    private Branch branch;
    private ArticyFlowPlayer flowPlayer;

    public void AssignBranch(
        ArticyFlowPlayer aFlowPlayer,
        Branch aBranch)
    {
        branch = aBranch;
        flowPlayer = aFlowPlayer;

        buttonText.text = "";

        // Get the choice text from Articy
        var objectWithMenuText =
            aBranch.Target as IObjectWithLocalizableMenuText;

        if (objectWithMenuText != null)
        {
            buttonText.text = objectWithMenuText.MenuText;
        }

        // Fallback if Articy does not have MenuText
        if (string.IsNullOrEmpty(buttonText.text))
        {
            buttonText.text = "Continue";
        }
    }

    public void OnBranchSelected()
    {
        if (flowPlayer == null)
            return;

        flowPlayer.Play(branch);
    }
}