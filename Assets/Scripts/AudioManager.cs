using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDatabaseSO audioDB;
    [SerializeField] private AudioSource bgmSource;

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

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = data.volume;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log($"Now playing music: {musicName}");
    }
}