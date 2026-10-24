using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 1f;
    [Tooltip("Off = a picture stretches to fill the screen. On = keeps its shape and leaves the game visible at the edges.")]
    [SerializeField] private bool preserveAspect = false;

    private Color baseColor = Color.black;
    private float alpha;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>(true);

        fadeImage.raycastTarget = false;
        SetLook(null);
        SetAlpha(0f);
    }

    // Fades to black, or to a picture if one is given. seconds <= 0 uses fadeSpeed.
    public IEnumerator FadeOut(Sprite picture = null, float seconds = 0f)
    {
        if (picture != null) SetLook(picture);
        else if (alpha <= 0.001f) SetLook(null);   // fresh plain fade: no old picture left over

        float speed = SpeedFor(seconds);
        while (alpha < 1f)
        {
            SetAlpha(Mathf.MoveTowards(alpha, 1f, Time.deltaTime * speed));
            yield return null;
        }
    }

    public IEnumerator FadeIn(float seconds = 0f)
    {
        float speed = SpeedFor(seconds);
        while (alpha > 0f)
        {
            SetAlpha(Mathf.MoveTowards(alpha, 0f, Time.deltaTime * speed));
            yield return null;
        }
        SetLook(null);   // back to black, ready for the next plain fade
    }

    public void SetBlack(Sprite picture = null)
    {
        Debug.Log($"[ScreenFader] SetBlack done alpha={alpha} color={fadeImage.color} imgEnabled={fadeImage.enabled} active={fadeImage.gameObject.activeInHierarchy}");
        StopAllCoroutines();
        SetLook(picture);
        SetAlpha(1f);
    }

    private float SpeedFor(float seconds) => seconds > 0f ? 1f / seconds : fadeSpeed;

    private void SetLook(Sprite picture)
    {
        fadeImage.sprite = picture;
        fadeImage.preserveAspect = picture != null && preserveAspect;
        baseColor = picture != null ? Color.white : Color.black;
        SetAlpha(alpha);   // reapply the color for the new look
    }

    private void SetAlpha(float a)
    {
        if (alpha >= 0.99f && a < 0.5f)
            Debug.Log($"[ScreenFader] leaving black\n{System.Environment.StackTrace}");

        alpha = a;
        Color c = baseColor;
        c.a = a;
        fadeImage.color = c;
    }
}