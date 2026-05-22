using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Seffaf cerceveyi izgaranin onune alir, izgarayi cerceve deligine
// dolduracak boyuta getirir, hayvan dokularini netlestirir.
// Cerceve boyutu DEGISMEZ.
// Unity menusu:  Tools > Cerceve On Plana Al
public static class FrameToFront
{
    [MenuItem("Tools/Cerceve On Plana Al")]
    public static void Apply()
    {
        var frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/jungle_frame.png");
        if (frameSprite == null)
        {
            EditorUtility.DisplayDialog("Hata", "jungle_frame.png import edilmemis. Bekleyip tekrar dene.", "Tamam");
            return;
        }

        var gameFrame = GameObject.Find("GameFrame");
        var gameArea  = GameObject.Find("Game Area");
        if (gameFrame == null || gameArea == null)
        {
            EditorUtility.DisplayDialog("Hata", "GameFrame veya Game Area bulunamadi (SampleScene'de calistir).", "Tamam");
            return;
        }
        var gm = Object.FindObjectOfType<GridManager>();
        if (gm == null) { EditorUtility.DisplayDialog("Hata", "GridManager bulunamadi.", "Tamam"); return; }

        // 1. cerceve: seffaf sprite + tiklama gecirsin + izgaranin onune
        var fImg = gameFrame.GetComponent<Image>();
        fImg.sprite = frameSprite;
        fImg.raycastTarget = false;
        gameFrame.transform.SetSiblingIndex(gameArea.transform.GetSiblingIndex() + 1);

        // 2. hucre boyutu: izgara cerceve deligini doldursun
        gm.cellSize = 138f;
        EditorUtility.SetDirty(gm);

        // 3. koyu panel: izgarayi sarsin, kenarlari vine altinda kalsin
        var gridRT = (RectTransform)gm.transform;
        gridRT.sizeDelta = new Vector2(800f, 1080f);

        // 4. hayvan dokulari netlik
        int fixedCount = FixAnimalTextures();

        EditorSceneManager.MarkSceneDirty(gameFrame.scene);
        EditorUtility.DisplayDialog("Tamam",
            "Hucre boyutu 138 yapildi (izgara cerceveyi dolduruyor).\n" +
            "Cerceve seffaf surumde, izgaranin onunde.\n" +
            fixedCount + " hayvan dokusu netlestirildi.\n\nCtrl+S ile kaydet.", "Tamam");
        Debug.Log("[FrameToFront] cellSize=" + gm.cellSize);
    }

    static int FixAnimalTextures()
    {
        int n = 0;
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/animals" });
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.filterMode = FilterMode.Bilinear;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
            n++;
        }
        return n;
    }
}
