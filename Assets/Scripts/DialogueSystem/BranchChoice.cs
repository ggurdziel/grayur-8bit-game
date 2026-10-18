using Articy.Unity;
using Articy.Unity.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BranchChoice : MonoBehaviour
{
    [SerializeField] private TMP_Text choiceText;
    [SerializeField] private Image arrow;

    public Branch Branch { get; private set; }

    public void Assign(Branch aBranch)
    {
        Branch = aBranch;

        string t = "";
        
        try { t = (aBranch.Target as IObjectWithLocalizableMenuText)?.MenuText; }
        catch { t = ""; }

        if (string.IsNullOrEmpty(t))
            try { t = (aBranch.Target as IObjectWithLocalizableText)?.Text; }
            catch { t = ""; }

        choiceText.text = string.IsNullOrEmpty(t) ? "Continue" : t.Trim();
        gameObject.SetActive(true);
        SetSelected(false);
    }

    public void Clear()
    {
        Branch = null;
        choiceText.text = "";
        SetSelected(false);
        gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (arrow != null)
            arrow.enabled = selected;
    }
}