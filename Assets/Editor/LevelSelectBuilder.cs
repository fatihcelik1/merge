using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

// Ana Menu'de bir "LEVELS" butonu ve scroll'lu LevelSelectPanel olusturur.
// Unity menusu:  Tools > Level Secim Ekranini Kur
public static class LevelSelectBuilder
{
    [MenuItem("Tools/Level Secim Ekranini Kur")]
    public static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede Canvas yok. Main Menu sahnesinde calistir.", "Tamam");
            return;
        }

        var ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ===== LevelSelectPanel =====
        RemoveIfExists(canvas.transform, "LevelSelectPanel");
        var panel = NewUI("LevelSelectPanel", canvas.transform);
        var pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0f, 0f, 0f, 0.7f);
        pImg.raycastTarget = true;
        Stretch(panel);
        panel.transform.SetAsLastSibling();

        // Box
        var box = NewImage("Box", panel.transform, ui, new Color(0.79f, 0.63f, 0.42f, 1f));
        SetRect(box, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(740f, 1140f));
        AddOutline(box, new Color(0.43f, 0.29f, 0.14f, 1f), 5f);

        // Header
        var header = NewText("Header", box.transform, "LEVELS", 64f,
            new Color(0.30f, 0.18f, 0f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
        SetRect(header.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(500f, 100f));

        // Back butonu (sag ust)
        var backGo = NewImage("BackButton", box.transform, ui, new Color(0.80f, 0.32f, 0.20f, 1f));
        SetRect(backGo, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(90f, 90f));
        var backBtn = backGo.AddComponent<Button>();
        backBtn.targetGraphic = backGo.GetComponent<Image>();
        AddOutline(backGo, new Color(0f, 0f, 0f, 0.4f), 4f);
        var xText = NewText("X", backGo.transform, "X", 50f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(xText.gameObject);

        // ScrollView
        var scroll = NewUI("ScrollView", box.transform);
        SetRect(scroll, new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(660f, 900f));
        var sr = scroll.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.inertia = true;
        sr.scrollSensitivity = 30f;

        // Viewport (mask + bg)
        var vp = NewImage("Viewport", scroll.transform, ui, new Color(0f, 0f, 0f, 0.18f));
        Stretch(vp);
        var mask = vp.AddComponent<Mask>();
        mask.showMaskGraphic = true;
        sr.viewport = (RectTransform)vp.transform;

        // Content (Grid + ContentSizeFitter)
        var content = NewUI("Content", vp.transform);
        var crt = (RectTransform)content.transform;
        crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0f, 0f);
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(130f, 130f);
        grid.spacing = new Vector2(14f, 14f);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.UpperCenter;
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = (RectTransform)content.transform;

        // LevelSelectManager
        var mgr = panel.AddComponent<LevelSelectManager>();
        mgr.content = (RectTransform)content.transform;
        mgr.panel = panel;
        mgr.buttonSprite = ui;

        // Back butonu -> Close
        UnityAction closeCall = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), mgr, "Close");
        UnityEventTools.AddPersistentListener(backBtn.onClick, closeCall);

        panel.SetActive(false);

        // ===== Ana menuye "LEVELS" butonu =====
        RemoveIfExists(canvas.transform, "LevelsButton");
        var lvlBtnGo = NewImage("LevelsButton", canvas.transform, ui, new Color(0.95f, 0.68f, 0.22f, 1f));
        var lvlImg = lvlBtnGo.GetComponent<Image>();
        var lvlBtn = lvlBtnGo.AddComponent<Button>();
        lvlBtn.targetGraphic = lvlImg;
        AddOutline(lvlBtnGo, new Color(0f, 0f, 0f, 0.4f), 4f);

        var playBtn = GameObject.Find("PlayButton");
        var lrt = (RectTransform)lvlBtnGo.transform;
        if (playBtn != null)
        {
            var playRt = (RectTransform)playBtn.transform;
            lrt.anchorMin = playRt.anchorMin;
            lrt.anchorMax = playRt.anchorMax;
            lrt.pivot = playRt.pivot;
            lrt.sizeDelta = new Vector2(Mathf.Max(playRt.sizeDelta.x * 0.85f, 360f), 110f);
            lrt.anchoredPosition = playRt.anchoredPosition + new Vector2(0f, -playRt.sizeDelta.y * 0.55f - 40f);
        }
        else
        {
            lrt.anchorMin = new Vector2(0.5f, 0.5f);
            lrt.anchorMax = new Vector2(0.5f, 0.5f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.sizeDelta = new Vector2(420f, 110f);
            lrt.anchoredPosition = new Vector2(0f, -250f);
        }

        var lvlTxt = NewText("Label", lvlBtnGo.transform, "LEVELS", 40f,
            new Color(0.30f, 0.18f, 0f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(lvlTxt.gameObject);

        // LevelsButton onClick -> panel.SetActive(true)
        UnityAction<bool> setActiveCall = (UnityAction<bool>)System.Delegate.CreateDelegate(typeof(UnityAction<bool>), panel, "SetActive");
        UnityEventTools.AddBoolPersistentListener(lvlBtn.onClick, setActiveCall, true);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorUtility.DisplayDialog("Tamam",
            "LevelSelectPanel olusturuldu + ana menuye 'LEVELS' butonu eklendi.\n\nCtrl+S ile kaydet.", "Tamam");
        Debug.Log("[LevelSelectBuilder] Tamamlandi.");
    }

    // ===== helpers =====
    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject NewImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var go = NewUI(name, parent);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        if (sprite != null) img.type = Image.Type.Sliced;
        return go;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, string text, float fontSize,
        Color color, FontStyles style, TextAlignmentOptions align)
    {
        var go = NewUI(name, parent);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    static void SetRect(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(GameObject go)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void AddOutline(GameObject go, Color color, float distance)
    {
        var ol = go.AddComponent<Outline>();
        ol.effectColor = color;
        ol.effectDistance = new Vector2(distance, -distance);
    }

    static void RemoveIfExists(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }
}
