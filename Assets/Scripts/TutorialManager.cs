using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Ilk oyun acilisinda gosterilen tutorial.
// NEW FRIEND'deki gibi iris karartma - yan yana iki hayvan etrafi bos kalir.
// Uzerlerinde pulse halkalar + ust yazi + skip butonu.
// Ilk basarili merge sonrasi otomatik kapanir.
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    GameObject ui;
    bool isShowing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("TutorialManager");
        go.AddComponent<TutorialManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sadece oyun (gameplay) sahnesinde tetiklenmeli, Splash veya Main Menu'de degil
        if (scene.name == "Main Menu" || scene.name == "Splash") return;
        if (PlayerPrefs.GetInt("tutorialDone", 0) != 0) return;
        StartCoroutine(StartTutorialDelayed());
    }

    IEnumerator StartTutorialDelayed()
    {
        yield return new WaitForSeconds(1.5f); // initial spawn bitsin
        if (isShowing) yield break;
        if (PlayerPrefs.GetInt("tutorialDone", 0) != 0) yield break;
        ShowTutorial();
    }

    void ShowTutorial()
    {
        isShowing = true;

        // Komsu cift bul (oncelik), yoksa rastgele
        Transform t1 = null, t2 = null;
        if (GridManager.Instance != null && GridManager.Instance.TryGetNeighborPair(out var n1, out var n2))
        {
            t1 = n1; t2 = n2;
        }
        else
        {
            var items = FindObjectsOfType<ItemData>();
            var groups = items.GroupBy(i => i.level).Where(g => g.Count() >= 2).ToList();
            if (groups.Count > 0)
            {
                var pair = groups[Random.Range(0, groups.Count)].Take(2).ToArray();
                t1 = pair[0].transform;
                t2 = pair[1].transform;
            }
        }
        if (t1 == null || t2 == null) { isShowing = false; return; } // hayvan yok = sahne uygun degil, flag set etme

        // Ana root canvas bul
        Canvas canvas = t1.GetComponentInParent<Canvas>();
        if (canvas != null) canvas = canvas.rootCanvas;
        if (canvas == null)
        {
            foreach (var c in FindObjectsOfType<Canvas>())
                if (c.isRootCanvas) { canvas = c; break; }
        }
        if (canvas == null) { isShowing = false; return; } // canvas yok = sahne uygun degil, flag set etme

        var canvasRt = (RectTransform)canvas.transform;
        Vector2 canvasSize = canvasRt.rect.size;

        // Hayvan local pozisyonlari
        Vector2 p1 = WorldToCanvasLocal(canvasRt, canvas.worldCamera, t1.position);
        Vector2 p2 = WorldToCanvasLocal(canvasRt, canvas.worldCamera, t2.position);

        // Tek hayvan ortalama boyut tahmini
        var t1Rt = t1 as RectTransform;
        float itemW = (t1Rt != null ? t1Rt.rect.width * t1.lossyScale.x / canvasRt.lossyScale.x : 170f);
        float itemH = (t1Rt != null ? t1Rt.rect.height * t1.lossyScale.y / canvasRt.lossyScale.y : 170f);

        // Iki hayvani kapsayan bounding box + padding
        float pad = 30f;
        float boxLeft   = Mathf.Min(p1.x, p2.x) - itemW * 0.5f - pad;
        float boxRight  = Mathf.Max(p1.x, p2.x) + itemW * 0.5f + pad;
        float boxBottom = Mathf.Min(p1.y, p2.y) - itemH * 0.5f - pad;
        float boxTop    = Mathf.Max(p1.y, p2.y) + itemH * 0.5f + pad;
        float boxCenterY = (boxTop + boxBottom) * 0.5f;
        float boxH = boxTop - boxBottom;

        // Root UI
        ui = NewUI("TutorialUI", canvas.transform);
        ui.transform.SetAsLastSibling();

        // 4 karartma paneli - iris
        Color darkC = new Color(0, 0, 0, 0.92f);
        float halfW = canvasSize.x * 0.5f;
        float halfH = canvasSize.y * 0.5f;

        // TOP
        var top = MakePanel("Top", ui.transform, darkC);
        var topRt = (RectTransform)top.transform;
        topRt.anchorMin = topRt.anchorMax = new Vector2(0.5f, 0.5f);
        topRt.pivot = new Vector2(0.5f, 0f);
        topRt.sizeDelta = new Vector2(canvasSize.x, halfH - boxTop);
        topRt.anchoredPosition = new Vector2(0, boxTop);

        // BOT
        var bot = MakePanel("Bot", ui.transform, darkC);
        var botRt = (RectTransform)bot.transform;
        botRt.anchorMin = botRt.anchorMax = new Vector2(0.5f, 0.5f);
        botRt.pivot = new Vector2(0.5f, 1f);
        botRt.sizeDelta = new Vector2(canvasSize.x, halfH + boxBottom);
        botRt.anchoredPosition = new Vector2(0, boxBottom);

        // LEFT
        var left = MakePanel("Left", ui.transform, darkC);
        var leftRt = (RectTransform)left.transform;
        leftRt.anchorMin = leftRt.anchorMax = new Vector2(0.5f, 0.5f);
        leftRt.pivot = new Vector2(1f, 0.5f);
        leftRt.sizeDelta = new Vector2(halfW + boxLeft, boxH);
        leftRt.anchoredPosition = new Vector2(boxLeft, boxCenterY);

        // RIGHT
        var right = MakePanel("Right", ui.transform, darkC);
        var rightRt = (RectTransform)right.transform;
        rightRt.anchorMin = rightRt.anchorMax = new Vector2(0.5f, 0.5f);
        rightRt.pivot = new Vector2(0f, 0.5f);
        rightRt.sizeDelta = new Vector2(halfW - boxRight, boxH);
        rightRt.anchoredPosition = new Vector2(boxRight, boxCenterY);

        // Ust yazi (bos alanin ustunde, top panel'in icinde)
        var title = NewUI("TutorialText", ui.transform);
        var tRt = (RectTransform)title.transform;
        tRt.anchorMin = tRt.anchorMax = new Vector2(0.5f, 0.5f);
        tRt.pivot = new Vector2(0.5f, 0.5f);
        tRt.anchoredPosition = new Vector2(0, boxTop + 80f);
        tRt.sizeDelta = new Vector2(canvasSize.x - 60, 80);
        var txt = title.AddComponent<TextMeshProUGUI>();
        txt.text = "Tap two same animals to merge!";
        txt.fontSize = 38;
        txt.fontStyle = FontStyles.Normal;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.characterSpacing = 5f;
        txt.raycastTarget = false;

        // Skip buton sag ust
        var skip = NewUI("SkipButton", ui.transform);
        var sRt = (RectTransform)skip.transform;
        sRt.anchorMin = sRt.anchorMax = new Vector2(1, 1);
        sRt.pivot = new Vector2(1, 1);
        sRt.anchoredPosition = new Vector2(-30, -30);
        sRt.sizeDelta = new Vector2(170, 75);
        var sImg = skip.AddComponent<Image>();
        sImg.color = new Color(1f, 1f, 1f, 0.18f);
        var sBtn = skip.AddComponent<Button>();
        sBtn.onClick.AddListener(Finish);
        var sTxtGo = NewUI("SkipTxt", skip.transform);
        Stretch(sTxtGo);
        var sTxt = sTxtGo.AddComponent<TextMeshProUGUI>();
        sTxt.text = "Skip";
        sTxt.fontSize = 34;
        sTxt.fontStyle = FontStyles.Bold;
        sTxt.alignment = TextAlignmentOptions.Center;
        sTxt.color = Color.white;
        sTxt.raycastTarget = false;

        // Pulse halkalar - iki hayvanin uzerinde, offset ile
        StartCoroutine(PulseLoop(t1, 0f));
        StartCoroutine(PulseLoop(t2, 0.55f));
    }

    Vector2 WorldToCanvasLocal(RectTransform canvasRt, Camera cam, Vector3 worldPos)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, cam, out local);
        return local;
    }

    IEnumerator PulseLoop(Transform target, float startOffset)
    {
        yield return new WaitForSeconds(startOffset);
        while (isShowing && target != null)
        {
            SpawnPulseRing(target);
            yield return new WaitForSeconds(1.2f);
        }
    }

    void SpawnPulseRing(Transform target)
    {
        if (ui == null || target == null) return;
        var ring = NewUI("PulseRing", ui.transform);
        var rt = (RectTransform)ring.transform;
        rt.position = target.position;
        rt.sizeDelta = new Vector2(160, 160);
        var img = ring.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.85f);
        img.raycastTarget = false;
        StartCoroutine(PulseAnim(rt, img, target));
    }

    IEnumerator PulseAnim(RectTransform rt, Image img, Transform target)
    {
        float dur = 1.1f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (!isShowing || rt == null || img == null) yield break;
            float k = t / dur;
            float s = 1f + k * 1.4f;
            rt.localScale = new Vector3(s, s, 1f);
            if (target != null) rt.position = target.position;
            img.color = new Color(1f, 1f, 1f, 0.85f * (1f - k));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    public void NotifyMergeHappened()
    {
        if (!isShowing) return;
        Finish();
    }

    void Finish()
    {
        isShowing = false;
        PlayerPrefs.SetInt("tutorialDone", 1);
        PlayerPrefs.Save();
        if (ui != null) Destroy(ui);
        ui = null;
    }

    static GameObject MakePanel(string n, Transform p, Color c)
    {
        var g = NewUI(n, p);
        var img = g.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = true;
        return g;
    }

    static GameObject NewUI(string n, Transform p)
    {
        var g = new GameObject(n, typeof(RectTransform));
        g.layer = 5;
        g.transform.SetParent(p, false);
        return g;
    }

    static void Stretch(GameObject g)
    {
        var r = (RectTransform)g.transform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
