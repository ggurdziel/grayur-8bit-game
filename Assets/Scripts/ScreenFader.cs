using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeSpeed = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        if (fadeGroup == null)
            fadeGroup = GetComponentInChildren<CanvasGroup>(true);

        if (fadeGroup != null)
            fadeGroup.alpha = 0f;
    }

    public IEnumerator FadeOut()
    {
        while (fadeGroup.alpha < 1f)
        {
            fadeGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        fadeGroup.alpha = 1f;
    }

    public IEnumerator FadeIn()
    {
        while (fadeGroup.alpha > 0f)
        {
            fadeGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        fadeGroup.alpha = 0f;
    }
}