using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Cihaz uyumu: ust HUD'u kamera centigi disina iten SafeAreaTop'u
// Canvas'a ekler, Item gorselinin en-boy oranini korur.
// Unity menusu:  Tools > Cihaz Uyumu (Safe Area)
public static class DeviceFix
{
    [MenuItem("Tools/Cihaz Uyumu (Safe Area)")]
    public static void Apply()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede Canvas yok.", "Tamam");
            return;
        }

        // 1. SafeAreaTop bileseni
        bool added = false;
        if (canvas.GetComponent<SafeAreaTop>() == null)
        {
            canvas.gameObject.AddComponent<SafeAreaTop>();
            added = true;
        }

        // 2. Item gorseli en-boy oranini korusun
        bool aspectFixed = false;
        var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item.prefab");
        if (itemPrefab != null)
        {
            var img = itemPrefab.GetComponent<Image>();
            if (img != null && !img.preserveAspect)
            {
                img.preserveAspect = true;
                EditorUtility.SetDirty(itemPrefab);
                aspectFixed = true;
            }
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

        EditorUtility.DisplayDialog("Tamam",
            "SafeAreaTop " + (added ? "eklendi" : "zaten vardi") + ".\n" +
            "Item en-boy orani " + (aspectFixed ? "korunacak sekilde ayarlandi" : "zaten ayarliydi") + ".\n\n" +
            "Ctrl+S ile kaydet.", "Tamam");
        Debug.Log("[DeviceFix] tamamlandi.");
    }
}
