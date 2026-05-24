using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Daily Notification GameObject'ini sahneye ekler.
// Unity menusu:  Tools > Daily Notification Ekle
public static class NotificationSetup
{
    [MenuItem("Tools/Daily Notification Ekle")]
    public static void Build()
    {
        // Mevcut varsa kaldir
        var existing = Object.FindObjectOfType<DailyNotificationManager>();
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Zaten Var",
                "Sahnede zaten DailyNotificationManager var. Tekrar olusturulsun mu?",
                "Evet", "Hayir")) return;
            Object.DestroyImmediate(existing.gameObject);
        }

        var go = new GameObject("DailyNotificationManager");
        go.AddComponent<DailyNotificationManager>();

        EditorSceneManager.MarkSceneDirty(go.scene);
        EditorUtility.DisplayDialog("Tamam",
            "DailyNotificationManager sahneye eklendi.\n\n" +
            "Bu obje DontDestroyOnLoad olur, sadece Main Menu sahnesinde ekle.\n\n" +
            "Ctrl+S ile kaydet.", "Tamam");
        Selection.activeGameObject = go;
        Debug.Log("[NotificationSetup] Tamamlandi.");
    }
}
