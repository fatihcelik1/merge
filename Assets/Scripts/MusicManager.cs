using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip backgroundMusic;  // Inspector'dan atanabilir (geriye uyum)
    private AudioSource audioSource;
    private bool isMuted = false;

    // Otomatik bootstrap - oyun acilir acilmaz spawn olur (AdManager / TutorialManager gibi)
    // Boylece Main Menu sahnesinde de muzik calar, sadece Game sahnesinde degil.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("MusicManager");
        go.AddComponent<MusicManager>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inspector'dan atanmamissa Resources'tan yukle
        if (backgroundMusic == null)
            backgroundMusic = Resources.Load<AudioClip>("bg_music");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;  // splash'ta calmasin, sahne yuklenmesinde karar verilir
        audioSource.volume = 0.5f;

        // Kullanicinin onceki ayarini hatirla (varsa)
        if (PlayerPrefs.GetInt("musicMuted", 0) == 1)
        {
            isMuted = true;
            audioSource.mute = true;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Splash sahnesinde muzik calmasin (video kendi sesini calsin)
        if (scene.name == "Splash")
        {
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            return;
        }

        // Diger sahnelerde (Main Menu, SampleScene, vs.) muzik calsin
        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();
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

    // SettingsManager tarafindan cagrilir - sadece muzigi ac/kapat
    public void SetMusicOn(bool on)
    {
        if (audioSource != null) audioSource.mute = !on;
    }
}