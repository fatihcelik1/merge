using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Video;

// Editor menusu: Tools > Splash Sahnesi Kur
// Otomatik olarak Splash sahnesi olusturur, video player + script ekler,
// Build Settings'e ilk sahne olarak ayarlar.
public static class SplashSceneBuilder
{
    const string VideoPath = "Assets/Videos/fcgames_splash.mp4";
    const string ScenePath = "Assets/Scenes/Splash.unity";

    [MenuItem("Tools/Splash Sahnesi Kur")]
    public static void Build()
    {
        var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(VideoPath);
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "Video bulunamadi:\n" + VideoPath +
                "\n\nVideoyu Assets/Videos/ klasorune koy ve adi fcgames_splash.mp4 olsun.",
                "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Yeni bos sahne
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Main Camera (default ile gelir)
        var camGo = GameObject.Find("Main Camera");
        if (camGo == null)
        {
            camGo = new GameObject("Main Camera");
            camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
        }
        var cam = camGo.GetComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;

        // VideoPlayer
        var vpGo = new GameObject("VideoPlayer");
        var vp = vpGo.AddComponent<VideoPlayer>();
        vp.clip = clip;
        vp.playOnAwake = false;
        vp.isLooping = false;
        vp.renderMode = VideoRenderMode.CameraNearPlane;
        vp.targetCamera = cam;
        vp.audioOutputMode = VideoAudioOutputMode.Direct;
        vp.aspectRatio = VideoAspectRatio.FitInside;

        // SplashController
        var ctrl = vpGo.AddComponent<SplashController>();
        ctrl.videoPlayer = vp;
        ctrl.nextScene = "Main Menu";

        // Sahneyi kaydet
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);

        // Build Settings - Splash ilk sahne
        var list = new List<EditorBuildSettingsScene>();
        list.Add(new EditorBuildSettingsScene(ScenePath, true));
        foreach (var s in EditorBuildSettings.scenes)
            if (s.path != ScenePath) list.Add(s);
        EditorBuildSettings.scenes = list.ToArray();

        EditorUtility.DisplayDialog("Tamam",
            "Splash sahnesi olusturuldu:\n" + ScenePath +
            "\n\nBuild Settings'te ilk sahne olarak ayarlandi.\n" +
            "Build alip telefonda test edebilirsin.",
            "OK");
    }
}
