using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

// Ana menuye "HAYVANLAR" butonu + paneli ekler.
// Panel 10 hayvani, levelini ve birlestirme puanini gosterir.
// Unity menusu:  Tools > Hayvan Rehberi Kur
public static class AnimalGuideBuilder
{
    static readonly string[] AnimalPaths = {
        "Assets/Sprites/animals/01_mouse.png",
        "Assets/Sprites/animals/02_rabbit.png",
        "Assets/Sprites/animals/03_cat.png",
        "Assets/Sprites/animals/04_monkey.png",
        "Assets/Sprites/animals/05_dog.png",
        "Assets/Sprites/animals/06_panda.png",
        "Assets/Sprites/animals/07_lion.png",
        "Assets/Sprites/animals/08_giraffe.png",
        "Assets/Sprites/animals/09_hippo.png",
        "Assets/Sprites/animals/10_elephant.png",
    };

    [MenuItem("Tools/Hayvan Rehberi Kur")]
    public static void Build()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { EditorUtility.DisplayDialog("Hata", "Canvas yok. Main Menu sahnesi olmali.", "Tamam"); return; }

        var ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // === panel ===
        Remove(canvas.transform, "AnimalGuidePanel");
        var panel = NewUI("AnimalGuidePanel", canvas.transform);
        var pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0,0,0,0.7f);
        Stretch(panel);
        panel.transform.SetAsLastSibling();

        var box = NewImg("Box", panel.transform, ui, new Color(0.79f,0.63f,0.42f,1));
        SetRect(box, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(820,580));
        Outline(box, new Color(0.43f,0.29f,0.14f,1), 5);

        var header = NewText("Header", box.transform, "HAYVANLAR", 56,
            new Color(0.30f,0.18f,0,1), FontStyles.Bold, TextAlignmentOptions.Center);
        SetRect(header.gameObject, new Vector2(0.5f,1), new Vector2(0,-65), new Vector2(560,90));

        // back butonu
        var back = NewImg("BackButton", box.transform, ui, new Color(0.80f,0.32f,0.20f,1));
        SetRect(back, new Vector2(1,1), new Vector2(-30,-30), new Vector2(90,90));
        var backBtn = back.AddComponent<Button>();
        backBtn.targetGraphic = back.GetComponent<Image>();
        Outline(back, new Color(0,0,0,0.4f), 4);
        var xT = NewText("X", back.transform, "X", 50, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(xT.gameObject);

        // grid alani
        var grid = NewUI("Grid", box.transform);
        SetRect(grid, new Vector2(0.5f,0.5f), new Vector2(0,-50), new Vector2(780,440));
        var gl = grid.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(140,180);
        gl.spacing = new Vector2(14,14);
        gl.padding = new RectOffset(15,15,15,15);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 5;
        gl.childAlignment = TextAnchor.UpperCenter;

        // 10 hayvan karti
        for (int i = 0; i < 10; i++)
        {
            int lvl = i + 1;
            int puan = lvl * lvl * 10;
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(AnimalPaths[i]);

            var card = NewImg("Card" + lvl, grid.transform, ui, new Color(0.95f,0.88f,0.72f,1));
            Outline(card, new Color(0.43f,0.29f,0.14f,0.6f), 3);

            var imgGo = NewUI("Img", card.transform);
            SetRect(imgGo, new Vector2(0.5f,0.5f), new Vector2(0,25), new Vector2(115,115));
            var im = imgGo.AddComponent<Image>();
            im.sprite = sp;
            im.preserveAspect = true;
            im.raycastTarget = false;

            var lt = NewText("Lvl", card.transform, "Lv " + lvl, 22,
                new Color(0.30f,0.18f,0,1), FontStyles.Bold, TextAlignmentOptions.Center);
            SetRect(lt.gameObject, new Vector2(0.5f,0.5f), new Vector2(0,-58), new Vector2(130,26));

            var pt = NewText("Pts", card.transform, puan + "+", 18,
                new Color(0.50f,0.32f,0.05f,1), FontStyles.Bold, TextAlignmentOptions.Center);
            SetRect(pt.gameObject, new Vector2(0.5f,0.5f), new Vector2(0,-78), new Vector2(130,22));
        }

        var mgr = panel.AddComponent<AnimalGuidePanel>();
        mgr.panel = panel;
        var closeCall = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), mgr, "Close");
        UnityEventTools.AddPersistentListener(backBtn.onClick, closeCall);

        panel.SetActive(false);

        // === HAYVANLAR butonu (LEVELS'in altinda) ===
        Remove(canvas.transform, "AnimalsButton");
        var aboutSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Main Menu/about.png");
        var btnGo = NewUI("AnimalsButton", canvas.transform);
        var aImg = btnGo.AddComponent<Image>();
        aImg.sprite = aboutSp;
        aImg.color = Color.white;
        aImg.preserveAspect = true;
        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = aImg;

        var levels = GameObject.Find("LevelsButton");
        var brt = (RectTransform)btnGo.transform;
        if (levels != null)
        {
            var lvRt = (RectTransform)levels.transform;
            brt.anchorMin = lvRt.anchorMin;
            brt.anchorMax = lvRt.anchorMax;
            brt.pivot = lvRt.pivot;
            brt.sizeDelta = new Vector2(140,140);
            brt.anchoredPosition = lvRt.anchoredPosition + new Vector2(0, -lvRt.sizeDelta.y * 0.5f - 100);
        }
        else
        {
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f,0.5f);
            brt.pivot = new Vector2(0.5f,0.5f);
            brt.sizeDelta = new Vector2(140,140);
            brt.anchoredPosition = new Vector2(0,-450);
        }

        var openCall = (UnityAction<bool>)System.Delegate.CreateDelegate(typeof(UnityAction<bool>), panel, "SetActive");
        UnityEventTools.AddBoolPersistentListener(btn.onClick, openCall, true);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorUtility.DisplayDialog("Tamam", "HAYVANLAR butonu ve paneli kuruldu.\nCtrl+S kaydet.", "Tamam");
    }

    // helpers
    static GameObject NewUI(string n, Transform p)
    { var g = new GameObject(n, typeof(RectTransform)); g.layer = 5; g.transform.SetParent(p, false); return g; }

    static GameObject NewImg(string n, Transform p, Sprite s, Color c)
    { var g = NewUI(n, p); var i = g.AddComponent<Image>(); i.sprite = s; i.color = c; if (s != null) i.type = Image.Type.Sliced; return g; }

    static TextMeshProUGUI NewText(string n, Transform p, string t, float sz, Color c, FontStyles st, TextAlignmentOptions a)
    { var g = NewUI(n, p); var x = g.AddComponent<TextMeshProUGUI>(); x.text = t; x.fontSize = sz; x.color = c; x.fontStyle = st; x.alignment = a; x.raycastTarget = false; return x; }

    static void SetRect(GameObject g, Vector2 a, Vector2 pos, Vector2 sz)
    { var r = (RectTransform)g.transform; r.anchorMin = a; r.anchorMax = a; r.pivot = new Vector2(0.5f,0.5f); r.anchoredPosition = pos; r.sizeDelta = sz; }

    static void Stretch(GameObject g)
    { var r = (RectTransform)g.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero; }

    static void Outline(GameObject g, Color c, float d)
    { var o = g.AddComponent<Outline>(); o.effectColor = c; o.effectDistance = new Vector2(d,-d); }

    static void Remove(Transform p, string n)
    { var t = p.Find(n); if (t != null) Object.DestroyImmediate(t.gameObject); }
}
