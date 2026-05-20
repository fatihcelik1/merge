using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip backgroundMusic;
    private AudioSource audioSource;
    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }

    public void ToggleMusic()
    {
        isMuted = !isMuted;
        audioSource.mute = isMuted;
        PlayerPrefs.SetInt("musicMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    void Start()
    {
        isMuted = PlayerPrefs.GetInt("musicMuted", 0) == 1;
        audioSource.mute = isMuted;
    }
}