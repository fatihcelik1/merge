using UnityEngine;

// Ust HUD'u (level, coin, badge'ler, ayar) cihazin kamera centigi /
// safe-area disina iter. Canvas'a eklenir, oyun basinda bir kez calisir.
public class SafeAreaTop : MonoBehaviour
{
    static readonly string[] TopNames =
    {
        "LevelPanel", "CoinPanel", "MovesPanel", "TargetPanel",
        "MusicPanel", "Settings Button"
    };

    void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Rect sa = Screen.safeArea;
        float topInsetPx = Screen.height - (sa.y + sa.height);
        if (topInsetPx <= 1f) return; // centik yok

        float scale = canvas.scaleFactor;
        if (scale <= 0f) scale = 1f;
        float insetUnits = topInsetPx / scale;

        foreach (string n in TopNames)
        {
            var go = GameObject.Find(n);
            if (go == null) continue;
            var rt = go.transform as RectTransform;
            if (rt == null) continue;
            rt.anchoredPosition += new Vector2(0f, -insetUnits);
        }
    }
}
