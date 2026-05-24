using UnityEngine;
using System;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

// Oyun arka plana atildiginda / kapatildiginda kullaniciya
// 1, 2, 3 ve 7 gun sonrasi icin geri donus bildirimi planlar.
//
// Sahnede tek bir GameObject'e ekle (DontDestroyOnLoad olur).
// Tools menusunde bir "Test Notification (10 sn)" butonu da var.
public class DailyNotificationManager : MonoBehaviour
{
    public static DailyNotificationManager Instance;

    const string CHANNEL_ID = "merge_safari_daily";
    const string CHANNEL_NAME = "Daily Reminder";
    const string CHANNEL_DESC = "The animals are calling you back";

    // Notification copy - one per scheduled day
    static readonly (string title, string body)[] Messages = new (string, string)[]
    {
        ("Your animals miss you!",      "Your cute friends are waiting for you to come back."),
        ("New levels are waiting",      "Fresh jungle adventures are ready to be explored."),
        ("It has been a week!",         "Your animal collection wants to see you again. Come back!")
    };

    // Schedule offsets in days
    static readonly int[] DaysOffset = { 1, 3, 7 };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
#if UNITY_ANDROID && !UNITY_EDITOR
        SetupChannel();
        RequestPermissionIfNeeded();
#endif
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) ScheduleAll();
        else CancelAll(); // oyuna donuldugunde bekleyenleri sil
    }

    void OnApplicationQuit()
    {
        ScheduleAll();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void SetupChannel()
    {
        var ch = new AndroidNotificationChannel()
        {
            Id = CHANNEL_ID,
            Name = CHANNEL_NAME,
            Importance = Importance.Default,
            Description = CHANNEL_DESC,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(ch);
    }

    void RequestPermissionIfNeeded()
    {
        // Android 13+ icin POST_NOTIFICATIONS izni gerekli
        var req = new PermissionRequest();
    }
#endif

    public void ScheduleAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Once eski bekleyenleri temizle
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        DateTime now = DateTime.Now;
        // Her gun saat 19:00'da bildirim (ogleden sonra prime time)
        for (int i = 0; i < DaysOffset.Length && i < Messages.Length; i++)
        {
            DateTime fire = now.Date.AddDays(DaysOffset[i]).AddHours(19);
            // Eger fire zamani gecmise denk geliyorsa (cok geceyse), bir gun sonra
            if (fire <= now.AddMinutes(5)) fire = fire.AddDays(1);

            var n = new AndroidNotification()
            {
                Title = Messages[i].title,
                Text = Messages[i].body,
                FireTime = fire,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large",
                ShouldAutoCancel = true,
            };
            AndroidNotificationCenter.SendNotification(n, CHANNEL_ID);
        }
#endif
    }

    public void CancelAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
#endif
    }

    // Editor'de test icin
    [ContextMenu("Test 10 Second Notification")]
    public void TestNotification()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SetupChannel();
        var n = new AndroidNotification()
        {
            Title = "Test",
            Text = "10 seconds passed - notifications are working!",
            FireTime = DateTime.Now.AddSeconds(10),
            SmallIcon = "icon_small",
            ShouldAutoCancel = true,
        };
        AndroidNotificationCenter.SendNotification(n, CHANNEL_ID);
        Debug.Log("Test notification scheduled");
#else
        Debug.Log("Notifications only work on Android device build.");
#endif
    }
}

// Basit placeholder - Unity 6'da Android izin akisi modulu icin
#if UNITY_ANDROID && !UNITY_EDITOR
internal class PermissionRequest
{
    public PermissionRequest()
    {
        if (UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            return;
        UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
    }
}
#endif
