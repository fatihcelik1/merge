using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Ana menudeki Logo gorselini "Merge Safari" logosuyla degistirir.
// Unity menusu:  Tools > Logo Degistir (Merge Safari)
public static class LogoChanger
{
    [MenuItem("Tools/Logo Degistir (Merge Safari)")]
    public static void ChangeLogo()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/logo_mergesafari.png");
        if (sprite == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "logo_mergesafari.png import edilmemis. Bekleyip tekrar dene.", "Tamam");
            return;
        }

        var logo = GameObject.Find("Logo");
        if (logo == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "Sahnede 'Logo' objesi yok. Main Menu sahnesini acip tekrar calistir.", "Tamam");
            return;
        }

        var img = logo.GetComponent<Image>();
        if (img == null)
        {
            EditorUtility.DisplayDialog("Hata", "Logo objesinde Image yok.", "Tamam");
            return;
        }

        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;

        // Logo objesini sprite'in en-boy oranina gore boyutlandir
        var rt = (RectTransform)logo.transform;
        float aspect = sprite.rect.width / sprite.rect.height;
        float targetW = 760f;
        rt.sizeDelta = new Vector2(targetW, targetW / aspect);

        EditorUtility.SetDirty(img);
        EditorUtility.SetDirty(rt);
        EditorSceneManager.MarkSceneDirty(logo.scene);

        EditorUtility.DisplayDialog("Tamam",
            "Ana menu logosu 'Merge Safari' ile degistirildi.\n\nCtrl+S ile kaydet.", "Tamam");
        Debug.Log("[LogoChanger] Logo guncellendi.");
    }
}
