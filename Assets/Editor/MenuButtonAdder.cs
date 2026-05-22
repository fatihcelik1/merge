using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

// Oyun ekranina, Shuffle'in yanina "ana menuye don" butonu ekler.
// Unity menusu:  Tools > Ana Menu Butonu Ekle
public static class MenuButtonAdder
{
    [MenuItem("Tools/Ana Menu Butonu Ekle")]
    public static void AddButton()
    {
        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null) { EditorUtility.DisplayDialog("Hata", "Sahnede Canvas yok.", "Tamam"); return; }

        var lm = UnityEngine.Object.FindObjectOfType<LevelManager>();
        if (lm == null) { EditorUtility.DisplayDialog("Hata", "Sahnede LevelManager yok. Oyun sahnesinde (SampleScene) calistir.", "Tamam"); return; }

        var shuffle = GameObject.Find("ShuffleButton");
        if (shuffle == null) { EditorUtility.DisplayDialog("Hata", "ShuffleButton bulunamadi.", "Tamam"); return; }

        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/prew.png");
        if (icon == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "prew.png henuz import edilmemis. Birkac saniye bekleyip tekrar dene.", "Tamam");
            return;
        }

        // eski varsa sil
        var old = canvas.transform.Find("BackToMenuButton");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        // yeni buton
        var go = new GameObject("BackToMenuButton", typeof(RectTransform));
        go.layer = 5; // UI
        go.transform.SetParent(canvas.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(180f, 180f);
        rt.anchoredPosition = new Vector2(-110f, 180f); // Shuffle'in solu

        var img = go.AddComponent<Image>();
        img.sprite = icon;
        img.preserveAspect = true;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // fonksiyon: ana menuye don
        UnityAction call = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), lm, "GoToMenu");
        UnityEventTools.AddPersistentListener(btn.onClick, call);

        // Shuffle'i saga kaydir -> ikisi ekranda ortalanmis cift olur
        var srt = (RectTransform)shuffle.transform;
        srt.anchoredPosition = new Vector2(110f, srt.anchoredPosition.y);

        // panellerin altinda kalsin (Win/Lose paneli ustunu kapatsin)
        go.transform.SetSiblingIndex(shuffle.transform.GetSiblingIndex() + 1);

        EditorUtility.SetDirty(shuffle);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorUtility.DisplayDialog("Tamam",
            "BackToMenuButton eklendi (Shuffle'in soluna), GoToMenu'ye baglandi.\n" +
            "Shuffle saga kaydirildi, ikisi ortalandi.\n\nCtrl+S ile kaydet.", "Tamam");
        Debug.Log("[MenuButtonAdder] BackToMenuButton olusturuldu.");
    }
}
