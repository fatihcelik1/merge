using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Izgara alanini sadelestirir: yesil arka plani ust barla uyumlu
// koyu mavi-gri bir panelle kapatir, hucreleri yumusatir.
// Unity menusu:  Tools > Izgara UI Duzenle
public static class GridUITweak
{
    // ust bar rozetiyle uyumlu, hafif daha acik
    static readonly Color PanelBg = new Color(0.20f, 0.25f, 0.34f, 1f);
    // yumusak, hafif cukur hucre
    static readonly Color CellCol = new Color(0f, 0f, 0f, 0.24f);

    [MenuItem("Tools/Izgara UI Duzenle")]
    public static void Apply()
    {
        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // --- bos slot prefab: yumusak pastel hucre ---
        var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EmtySlot.prefab");
        if (slotPrefab != null)
        {
            var sImg = slotPrefab.GetComponent<Image>();
            if (sImg != null)
            {
                sImg.sprite = uiSprite;
                sImg.type = Image.Type.Sliced;
                sImg.color = CellCol;
                EditorUtility.SetDirty(slotPrefab);
            }
        }
        AssetDatabase.SaveAssets();

        // --- izgara arka plan paneli ---
        var gm = Object.FindObjectOfType<GridManager>();
        if (gm == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede GridManager yok (SampleScene'de calistir).", "Tamam");
            return;
        }

        var grid = gm.gameObject;
        var gImg = grid.GetComponent<Image>();
        if (gImg == null) gImg = grid.AddComponent<Image>();
        gImg.enabled = true;
        gImg.sprite = uiSprite;
        gImg.type = Image.Type.Sliced;
        gImg.color = PanelBg;
        gImg.raycastTarget = false;

        var rt = (RectTransform)grid.transform;
        rt.sizeDelta = new Vector2(812f, 1138f);

        EditorSceneManager.MarkSceneDirty(grid.scene);
        EditorUtility.DisplayDialog("Tamam",
            "Izgara arka plani ust barla uyumlu koyu panele cevrildi, " +
            "hucreler yumusatildi.\n\nCtrl+S ile kaydet.", "Tamam");
        Debug.Log("[GridUITweak] Izgara UI guncellendi.");
    }
}
