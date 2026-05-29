using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Test APK olusturur, E:\merge_build klasorune kaydeder (C: dolu).
// Unity menusu:  Tools > APK Olustur (E: diske)
// Production AAB olusturur, Play Store icin imzali paket.
// Unity menusu:  Tools > AAB Olustur (Production)
public static class BuildScript
{
    const string OutPath = @"E:\merge_build\merge.apk";
    const string AabPath = @"E:\merge_build\mergesafari.aab";

    [MenuItem("Tools/APK Olustur (E: diske)")]
    public static void BuildAndroid()
    {
        // hedef klasoru garanti et
        var dir = System.IO.Path.GetDirectoryName(OutPath);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Hata",
                "Build Settings'te etkin sahne yok.", "Tamam");
            return;
        }

        // test APK: debug imzalama (parola gerekmez), sonra eski ayar geri
        bool prevCustomKeystore = PlayerSettings.Android.useCustomKeystore;
        PlayerSettings.Android.useCustomKeystore = false;

        // AAB degil GERCEK APK uret (AAB telefona kurulamaz)
        bool prevAppBundle = EditorUserBuildSettings.buildAppBundle;
        EditorUserBuildSettings.buildAppBundle = false;

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        BuildReport report;
        try
        {
            report = BuildPipeline.BuildPlayer(options);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = prevCustomKeystore;
            EditorUserBuildSettings.buildAppBundle = prevAppBundle;
        }

        if (report.summary.result == BuildResult.Succeeded)
        {
            float mb = report.summary.totalSize / 1048576f;
            Debug.Log("[BuildScript] APK hazir: " + OutPath + "  (" + mb.ToString("0.0") + " MB)");
            EditorUtility.RevealInFinder(OutPath);
            EditorUtility.DisplayDialog("APK Hazir",
                "APK olusturuldu:\n" + OutPath + "\n\nBoyut: " + mb.ToString("0.0") + " MB\n\n" +
                "Telefona kopyalayip kurabilirsin.", "Tamam");
        }
        else
        {
            EditorUtility.DisplayDialog("Build Basarisiz",
                "APK olusturulamadi. Console'daki kirmizi hatalara bak.\n\n" +
                "En sik sebep: Android Build Support modulu kurulu degil " +
                "(Unity Hub > Installs > modul ekle).", "Tamam");
        }
    }

    [MenuItem("Tools/AAB Olustur (Production)")]
    public static void BuildAab()
    {
        // Keystore sifresi girilmis mi kontrol et
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
        {
            EditorUtility.DisplayDialog("Sifre Eksik",
                "Production keystore sifresi girilmemis.\n\n" +
                "Edit > Project Settings > Player > Publishing Settings\n" +
                "Keystore password ve Key password alanlarini doldur, sonra tekrar dene.\n\n" +
                "(Sifreler her Unity oturumunda yeniden girilmeli - Unity guvenlik icin cache'lemez.)", "Tamam");
            return;
        }

        var dir = System.IO.Path.GetDirectoryName(AabPath);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Hata",
                "Build Settings'te etkin sahne yok.", "Tamam");
            return;
        }

        // Production: imzali + AAB
        bool prevCustomKeystore = PlayerSettings.Android.useCustomKeystore;
        bool prevAppBundle = EditorUserBuildSettings.buildAppBundle;
        PlayerSettings.Android.useCustomKeystore = true;
        EditorUserBuildSettings.buildAppBundle = true;

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = AabPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        BuildReport report;
        try
        {
            report = BuildPipeline.BuildPlayer(options);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = prevCustomKeystore;
            EditorUserBuildSettings.buildAppBundle = prevAppBundle;
        }

        if (report.summary.result == BuildResult.Succeeded)
        {
            float mb = report.summary.totalSize / 1048576f;
            Debug.Log("[BuildScript] AAB hazir: " + AabPath + "  (" + mb.ToString("0.0") + " MB)");
            EditorUtility.RevealInFinder(AabPath);
            EditorUtility.DisplayDialog("AAB Hazir (Production)",
                "Play Store icin imzali AAB olusturuldu:\n" + AabPath +
                "\n\nBoyut: " + mb.ToString("0.0") + " MB\n\n" +
                "Play Console > Test edin ve yayinlayin > Dahili test\n" +
                "veya Kapali test ekranina yukle.", "Tamam");
        }
        else
        {
            EditorUtility.DisplayDialog("Build Basarisiz",
                "AAB olusturulamadi. Console'daki kirmizi hatalara bak.\n\n" +
                "Olasi sebepler:\n" +
                "- Keystore sifresi yanlis\n" +
                "- Alias bulunamadi\n" +
                "- Keystore dosyasi yok", "Tamam");
        }
    }
}
