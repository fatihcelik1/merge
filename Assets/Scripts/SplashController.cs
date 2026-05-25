using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

// Splash sahnesinde VideoPlayer'i izler, video bitince Main Menu'ye gecer.
// Dokunma ile atlanabilir. Failsafe sure: video oynamasa bile max sn sonra menuye atar.
public class SplashController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextScene = "Main Menu";
    public float maxDuration = 8f;

    bool transitioning = false;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += _ => GoToMenu();
            videoPlayer.errorReceived += (vp, msg) => { Debug.LogWarning("[Splash] Video error: " + msg); GoToMenu(); };
            videoPlayer.Play();
        }
        Invoke(nameof(GoToMenu), maxDuration);
    }

    void Update()
    {
        bool tapped = false;
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) tapped = true;
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) tapped = true;
        if (tapped) GoToMenu();
    }

    void GoToMenu()
    {
        if (transitioning) return;
        transitioning = true;
        CancelInvoke();
        SceneManager.LoadScene(nextScene);
    }
}
