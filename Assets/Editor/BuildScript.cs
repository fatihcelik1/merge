using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Test APK olusturur, E:\merge_build klasorune kaydeder (C: dolu).
// Unity menusu:  Tools > APK Olustur (E: diske)
public static class BuildScript
{
    const string OutPath = @"E:\merge_build\merge.apk";

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
}
