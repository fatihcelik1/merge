using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;

// Win / Lose panellerini "Klasik" sablona gore sahneye kurar.
// Unity menusu:  Tools > Win-Lose Panel Olustur
public static class WinLosePanelBuilder
{
    // --- renkler ---
    static readonly Color Dim      = new Color(0f, 0f, 0f, 0.66f);
    static readonly Color Wood     = new Color(0.79f, 0.63f, 0.42f, 1f);
    static readonly Color WoodEdge = new Color(0.43f, 0.29f, 0.14f, 1f);
    static readonly Color Gold     = new Color(1f, 0.81f, 0.20f, 1f);
    static readonly Color RedRib   = new Color(0.80f, 0.27f, 0.17f, 1f);
    static readonly Color Green    = new Color(0.40f, 0.68f, 0.22f, 1f);
    static readonly Color Grey     = new Color(0.57f, 0.45f, 0.28f, 1f);
    static readonly Color DarkSlot = new Color(0f, 0f, 0f, 0.28f);
    static readonly Color BrownTxt = new Color(0.35f, 0.20f, 0.06f, 1f);

    static Sprite uiSprite;   // yuvarlatilmis kose (sliced)
    static Sprite knob;       // yuvarlak (coin / star fallback)
    static Sprite starSprite; // oyundan bulunan yildiz

    [MenuItem("Tools/Win-Lose Panel Olustur")]
    public static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { EditorUtility.DisplayDialog("Hata", "Sahnede Canvas bulunamadi.", "Tamam"); return; }

        var lm = Object.FindObjectOfType<LevelManager>();
        if (lm == null) { EditorUtility.DisplayDialog("Hata", "Sahnede LevelManager bulunamadi.", "Tamam"); return; }

        uiSprite   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knob       = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        starSprite = GrabSprite("LevelPanel") ?? knob;

        // varsa eskilerini temizle
        RemoveIfExists(canvas.transform, "WinPanel");
        RemoveIfExists(canvas.transform, "LosePanel");

        GameObject win  = BuildWin(canvas.transform, lm);
        GameObject lose = BuildLose(canvas.transform, lm);

        // LevelManager alanlarini bagla
        var so = new SerializedObject(lm);
        so.FindProperty("winPanel").objectReferenceValue = win;
        so.FindProperty("losePanel").objectReferenceValue = lose;
        so.FindProperty("winRewardText").objectReferenceValue =
            win.transform.Find("Box/RewardBadge/RewardText").GetComponent<TextMeshProUGUI>();
        so.FindProperty("loseProgressText").objectReferenceValue =
            lose.transform.Find("Box/ProgressText").GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        win.SetActive(false);
        lose.SetActive(false);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorUtility.DisplayDialog("Tamam",
            "WinPanel ve LosePanel olusturuldu, LevelManager'a baglandi.\n\n" +
            "Sahneyi kaydetmeyi unutma (Ctrl+S).", "Tamam");
        Debug.Log("[WinLosePanelBuilder] Paneller olusturuldu.");
    }

    // ---------------- WIN ----------------
    static GameObject BuildWin(Transform canvas, LevelManager lm)
    {
        GameObject panel = Panel(canvas, "WinPanel");
        GameObject box   = Box(panel.transform);

        Ribbon(box.transform, "LEVEL COMPLETE!", Gold, BrownTxt);

        // tek parca uclu yildiz (kendi sprite'inla degistirebilirsin)
        GameObject stars = Img(box.transform, "Stars", starSprite, Gold);
        SetRect(stars, new Vector2(0.5f, 0.5f), new Vector2(0, 122), new Vector2(380, 132));
        var starsImg = stars.GetComponent<Image>();
        starsImg.type = Image.Type.Simple;
        starsImg.preserveAspect = true;

        // odul rozeti
        GameObject badge = Img(box.transform, "RewardBadge", uiSprite, DarkSlot);
        SetRect(badge, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(340, 96));
        GameObject coin = Img(badge.transform, "Coin", knob, Gold);
        SetRect(coin, new Vector2(0.5f, 0.5f), new Vector2(-104, 0), new Vector2(56, 56));
        TextMeshProUGUI rew = Text(badge.transform, "RewardText", "+0", 44, Gold, TextAlignmentOptions.Left);
        SetRect(rew.gameObject, new Vector2(0.5f, 0.5f), new Vector2(40, 0), new Vector2(220, 70));

        // butonlar
        GameObject menu = Button(box.transform, "MenuButton", "MENU", Grey,  new Vector2(-168, -208));
        GameObject next = Button(box.transform, "NextButton", "NEXT", Green, new Vector2(168, -208));
        Wire(menu, lm, "GoToMenu");
        Wire(next, lm, "NextLevel");

        return panel;
    }

    // ---------------- LOSE ----------------
    static GameObject BuildLose(Transform canvas, LevelManager lm)
    {
        GameObject panel = Panel(canvas, "LosePanel");
        GameObject box   = Box(panel.transform);

        Ribbon(box.transform, "OUT OF MOVES", RedRib, Color.white);

        TextMeshProUGUI info = Text(box.transform, "InfoText", "Target not reached", 30, BrownTxt, TextAlignmentOptions.Center);
        SetRect(info.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0, 96), new Vector2(440, 50));

        TextMeshProUGUI prog = Text(box.transform, "ProgressText", "0 / 0", 60, new Color(0.48f, 0.18f, 0.11f), TextAlignmentOptions.Center);
        SetRect(prog.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0, 18), new Vector2(440, 80));

        GameObject menu  = Button(box.transform, "MenuButton",  "MENU",  Grey,  new Vector2(-168, -208));
        GameObject retry = Button(box.transform, "RetryButton", "RETRY", Green, new Vector2(168, -208));
        Wire(menu,  lm, "GoToMenu");
        Wire(retry, lm, "RetryLevel");

        return panel;
    }

    // ---------------- yapi tasi fonksiyonlari ----------------
    static GameObject Panel(Transform canvas, string name)
    {
        GameObject go = NewUI(name, canvas);
        var img = go.AddComponent<Image>();
        img.color = Dim;
        img.raycastTarget = true; // arkadaki tiklamalari engeller
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.transform.SetAsLastSibling();
        return go;
    }

    static GameObject Box(Transform parent)
    {
        GameObject box = Img(parent, "Box", uiSprite, Wood);
        SetRect(box, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 580));
        var outline = box.AddComponent<Outline>();
        outline.effectColor = WoodEdge;
        outline.effectDistance = new Vector2(5, -5);
        return box;
    }

    static void Ribbon(Transform box, string title, Color ribColor, Color txtColor)
    {
        GameObject rib = Img(box, "Ribbon", uiSprite, ribColor);
        SetRect(rib, new Vector2(0.5f, 0.5f), new Vector2(0, 258), new Vector2(560, 108));
        TextMeshProUGUI t = Text(rib.transform, "RibbonText", title, 42, txtColor, TextAlignmentOptions.Center);
        SetStretch(t.gameObject);
    }

    static GameObject Button(Transform box, string name, string label, Color col, Vector2 pos)
    {
        GameObject go = Img(box, name, uiSprite, col);
        SetRect(go, new Vector2(0.5f, 0.5f), pos, new Vector2(304, 100));
        var img = go.GetComponent<Image>();
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var ol = go.AddComponent<Outline>();
        ol.effectColor = new Color(0f, 0f, 0f, 0.35f);
        ol.effectDistance = new Vector2(0, -4);
        TextMeshProUGUI t = Text(go.transform, "Label", label, 34, Color.white, TextAlignmentOptions.Center);
        SetStretch(t.gameObject);
        return go;
    }

    static void Wire(GameObject buttonGo, LevelManager lm, string method)
    {
        var btn = buttonGo.GetComponent<Button>();
        UnityAction call = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), lm, method);
        UnityEventTools.AddPersistentListener(btn.onClick, call);
    }

    // ---------------- yardimcilar ----------------
    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; // UI
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject Img(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject go = NewUI(name, parent);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        if (sprite != null) img.type = Image.Type.Sliced;
        return go;
    }

    static TextMeshProUGUI Text(Transform parent, string name, string content, float size, Color color, TextAlignmentOptions align)
    {
        GameObject go = NewUI(name, parent);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        return t;
    }

    static void SetRect(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void SetStretch(GameObject go)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static Sprite GrabSprite(string objName)
    {
        foreach (var img in Resources.FindObjectsOfTypeAll<Image>())
        {
            if (img.gameObject.name == objName && img.sprite != null && img.gameObject.scene.IsValid())
                return img.sprite;
        }
        return null;
    }

    static void RemoveIfExists(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }
}
