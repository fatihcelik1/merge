using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Tum UI butonlarina otomatik tiklama sesi ekler.
// Sahneye obje koymaya gerek yok - oyun basinda kendini kurar.
// Ses, Settings'teki "Sound" ayarina baglidir.
public class ClickSound : MonoBehaviour
{
    static ClickSound _instance;
    AudioSource _audio;
    AudioClip _clip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("ClickSound");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<ClickSound>();
    }

    void Awake()
    {
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _clip = Resources.Load<AudioClip>("click");

        SceneManager.sceneLoaded += OnSceneLoaded;
        HookButtons();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookButtons();
    }

    void HookButtons()
    {
        var buttons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            // ayni butona iki kez eklenmesini onle
            b.onClick.RemoveListener(PlayClick);
            b.onClick.AddListener(PlayClick);
        }
    }

    void PlayClick()
    {
        if (_clip == null || _audio == null) return;
        if (SettingsManager.Instance != null && !SettingsManager.Instance.soundOn) return;
        _audio.PlayOneShot(_clip);
    }
}
