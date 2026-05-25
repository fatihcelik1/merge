using UnityEditor;
using UnityEngine;

// Editor menusu: Tools > Reset Progress (En Bastan Basla)
// Tum PlayerPrefs verisini siler - level, para, gorulen hayvanlar, vs.
// Sadece Editor'da. Telefon APK'sini etkilemez (orada Settings > Apps > Clear Data lazim).
public static class ResetProgress
{
    [MenuItem("Tools/Reset Progress (En Bastan Basla)")]
    public static void Reset()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Reset Progress",
            "Tum oyun verisi silinecek:\n" +
            "- Level ve para\n" +
            "- Shuffle hakki\n" +
            "- Gorulen hayvanlar (NEW FRIEND tekrar tetiklenir)\n" +
            "- Tum ayarlar\n\n" +
            "Devam edilsin mi?",
            "Evet sil",
            "Iptal");
        if (!ok) return;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[ResetProgress] Tum PlayerPrefs silindi. Play tusuna basinca oyun en bastan baslar.");
        EditorUtility.DisplayDialog("Tamam", "Veriler silindi.\nPlay tusuna basinca oyun en bastan baslar.", "OK");
    }
}
