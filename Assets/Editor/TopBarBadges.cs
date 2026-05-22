using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

// Ust bardaki Moves ve Target yazilarini ikonlu koyu rozetlere cevirir.
// Unity menusu:  Tools > Ust Bar Rozetlerini Kur
public static class TopBarBadges
{
    static readonly Color BadgeBg   = new Color(0.16f, 0.21f, 0.29f, 0.96f);
    static readonly Color BadgeRim  = new Color(0.38f, 0.47f, 0.58f, 1f);
    static readonly Color LabelCol  = new Color(0.66f, 0.75f, 0.87f, 1f);
    static readonly Color NumberCol = Color.white;

    // rozet olculeri
    const float BadgeW = 264f;
    const float BadgeH = 90f;

    [MenuItem("Tools/Ust Bar Rozetlerini Kur")]
    public static void Build()
    {
        var lm = Object.FindObjectOfType<LevelManager>();
        if (lm == null) { EditorUtility.DisplayDialog("Hata", "Sahnede LevelManager yok (SampleScene'de calistir).", "Tamam"); return; }
        if (lm.movesText == null || lm.targetText == null)
        {
            EditorUtility.DisplayDialog("Hata", "LevelManager'da movesText / targetText atanmamis.", "Tamam");
            return;
        }

        Sprite coin  = GrabSprite("CoinIcon");
        Sprite moves = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/moves_icon.png");

        StyleBadge(lm.movesText,  "MOVES",  new Vector2(-138f, -162f), moves);
        StyleBadge(lm.targetText, "TARGET", new Vector2( 138f, -162f), coin);

        EditorSceneManager.MarkSceneDirty(lm.gameObject.scene);
        EditorUtility.DisplayDialog("Tamam",
            "Moves ve Target rozetleri kuruldu." +
            (moves == null ? "\n\nUYARI: moves_icon.png import edilmemis, bekleyip tekrar dene." : "") +
            (coin  == null ? "\n\nUYARI: CoinIcon sprite'i bulunamadi." : "") +
            "\n\nCtrl+S ile kaydet.", "Tamam");
        Debug.Log("[TopBarBadges] Rozetler olusturuldu.");
    }

    static void StyleBadge(TextMeshProUGUI numberText, string label, Vector2 panelPos, Sprite iconSprite)
    {
        GameObject panel = numberText.transform.parent.gameObject;

        // --- rozet govdesi ---
        var rt = (RectTransform)panel.transform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(BadgeW, BadgeH);
        rt.anchoredPosition = panelPos;

        var bg = panel.GetComponent<Image>();
        if (bg == null) bg = panel.AddComponent<Image>();
        bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        bg.type = Image.Type.Sliced;
        bg.color = BadgeBg;
        bg.raycastTarget = false;

        var outline = panel.GetComponent<Outline>();
        if (outline == null) outline = panel.AddComponent<Outline>();
        outline.effectColor = BadgeRim;
        outline.effectDistance = new Vector2(3f, -3f);

        DestroyChild(panel, "Label");
        DestroyChild(panel, "Icon");

        bool hasIcon = iconSprite != null;
        // ikon solda; metin blogu sagda
        float iconX  = -BadgeW * 0.5f + 52f;          // sol kenardan iceri
        float textX  = hasIcon ? 36f : 0f;
        float textW  = hasIcon ? 150f : BadgeW - 40f;

        // --- ikon ---
        if (hasIcon)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.layer = 5;
            iconGo.transform.SetParent(panel.transform, false);
            var irt = (RectTransform)iconGo.transform;
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(58f, 58f);
            irt.anchoredPosition = new Vector2(iconX, 0f);
            var iimg = iconGo.AddComponent<Image>();
            iimg.sprite = iconSprite;
            iimg.preserveAspect = true;
            iimg.raycastTarget = false;
        }

        // --- etiket ---
        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.layer = 5;
        labelGo.transform.SetParent(panel.transform, false);
        var lrt = (RectTransform)labelGo.transform;
        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
        lrt.pivot = new Vector2(0.5f, 0.5f);
        lrt.sizeDelta = new Vector2(textW, 24f);
        lrt.anchoredPosition = new Vector2(textX, 23f);
        var lt = labelGo.AddComponent<TextMeshProUGUI>();
        lt.text = label;
        lt.fontSize = 16f;
        lt.color = LabelCol;
        lt.alignment = TextAlignmentOptions.Center;
        lt.fontStyle = FontStyles.Bold;
        lt.characterSpacing = 2f;
        lt.raycastTarget = false;

        // --- sayi (uzun degerlerde otomatik kuculur) ---
        var nrt = (RectTransform)numberText.transform;
        nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0.5f);
        nrt.pivot = new Vector2(0.5f, 0.5f);
        nrt.sizeDelta = new Vector2(textW, 50f);
        nrt.anchoredPosition = new Vector2(textX, -16f);
        numberText.color = NumberCol;
        numberText.alignment = TextAlignmentOptions.Center;
        numberText.fontStyle = FontStyles.Bold;
        numberText.raycastTarget = false;
        numberText.enableAutoSizing = true;
        numberText.fontSizeMin = 20f;
        numberText.fontSizeMax = 42f;

        EditorUtility.SetDirty(panel);
        EditorUtility.SetDirty(numberText);
    }

    static void DestroyChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
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
}
