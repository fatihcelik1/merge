using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public Sprite onSprite;
    public Sprite offSprite;

    public Image soundButtonImage;
    public Image musicButtonImage;
    public Image vibrationButtonImage;

    public bool soundOn = true;
    public bool musicOn = true;
    public bool vibrationOn = true;

    void Awake()
    {
        // SettingsManager sahneye ozel UI referanslari tutuyor (buton gorselleri),
        // bu yuzden DontDestroyOnLoad OLMAMALI. Her sahne kendi SettingsManager'ini
        // kullanir; ayarlar zaten PlayerPrefs uzerinden paylasiliyor.
        Instance = this;
        LoadSettings();
    }

    void Start()
    {
        UpdateButtons();
        ApplySettings();
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        PlayerPrefs.SetInt("soundOn", soundOn ? 1 : 0);
        UpdateButtons();
        ApplySettings();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        PlayerPrefs.SetInt("musicOn", musicOn ? 1 : 0);
        UpdateButtons();
        ApplySettings();
    }

    public void ToggleVibration()
    {
        vibrationOn = !vibrationOn;
        PlayerPrefs.SetInt("vibrationOn", vibrationOn ? 1 : 0);
        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (soundButtonImage != null)
            soundButtonImage.sprite = soundOn ? onSprite : offSprite;
        if (musicButtonImage != null)
            musicButtonImage.sprite = musicOn ? onSprite : offSprite;
        if (vibrationButtonImage != null)
            vibrationButtonImage.sprite = vibrationOn ? onSprite : offSprite;
    }

    void ApplySettings()
    {
        AudioListener.volume = musicOn ? 1f : 0f;
    }

    void LoadSettings()
    {
        soundOn = PlayerPrefs.GetInt("soundOn", 1) == 1;
        musicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;
        vibrationOn = PlayerPrefs.GetInt("vibrationOn", 1) == 1;
    }

    public void Vibrate()
    {
        if (vibrationOn)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
            #endif
        }
    }
}