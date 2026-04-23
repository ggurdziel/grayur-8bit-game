using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDatabaseSO audioDB;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float fadeDuration = 0.75f;

    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("AudioManager Start running");
        PlayMusic("MainTheme");
    }

    public void PlayMusic(string musicName)
    {
        var data = audioDB.Get(musicName);

        if (data == null)
        {
            Debug.LogWarning($"Music not found: {musicName}");
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null) return;

        if (bgmSource == null)
        {
            Debug.LogWarning("BGM AudioSource is not assigned.");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(FadeToMusic(clip, data.volume, musicName));
    }

    private IEnumerator FadeToMusic(AudioClip newClip, float targetVolume, string musicName)
    {
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;

            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
                yield return null;
            }

            bgmSource.volume = 0f;
            bgmSource.Stop();
        }

        bgmSource.clip = newClip;
        bgmSource.loop = true;
        bgmSource.Play();

        float fadeInTime = 0f;
        while (fadeInTime < fadeDuration)
        {
            fadeInTime += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, fadeInTime / fadeDuration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
        Debug.Log($"Now playing music: {musicName}");

        musicFadeCoroutine = null;
    }
}