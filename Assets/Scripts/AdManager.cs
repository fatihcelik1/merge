using UnityEngine;
using GoogleMobileAds.Api;
using System;

// AdMob reklam yoneticisi (singleton, DontDestroyOnLoad).
// Test reklam ID'leri ile calisir. Production'da kendi ID'lerle degisecek.
// - Banner: alt seritte gosterilir
// - Interstitial: ShowInterstitial() cagrisi ile (orn. level bitince)
// - Rewarded: ShowRewarded(callback) cagrisi ile (orn. Watch Ad butonu)
public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    // === PRODUCTION TOGGLE ===
    // true  -> Google'in test reklamlari (gelistirme - kendine tiklayabilirsin)
    // false -> Gercek reklamlar (YAYIN ONCESI false yap)
    const bool useTestAds = true;

    // Google'in resmi test ad unit ID'leri (her app'te calisir)
    const string TestBannerId       = "ca-app-pub-3940256099942544/6300978111";
    const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    const string TestRewardedId     = "ca-app-pub-3940256099942544/5224354917";

    // Gercek ad unit ID'leri (Merge Safari: Jungle, FCgames)
    const string RealBannerId       = "ca-app-pub-5924651773494504/3438261718";
    const string RealInterstitialId = "ca-app-pub-5924651773494504/4529879312";
    const string RealRewardedId     = "ca-app-pub-5924651773494504/7809128868";

    string BannerId       => useTestAds ? TestBannerId       : RealBannerId;
    string InterstitialId => useTestAds ? TestInterstitialId : RealInterstitialId;
    string RewardedId     => useTestAds ? TestRewardedId     : RealRewardedId;

    BannerView banner;
    InterstitialAd interstitial;
    RewardedAd rewarded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MobileAds.Initialize(status =>
        {
            Debug.Log("[AdManager] MobileAds initialized");
            LoadBanner();
            LoadInterstitial();
            LoadRewarded();
        });
    }

    // ============ BANNER ============
    void LoadBanner()
    {
        if (banner != null) banner.Destroy();
        banner = new BannerView(BannerId, AdSize.Banner, AdPosition.Bottom);
        banner.LoadAd(new AdRequest());
    }

    public void ShowBanner() { if (banner == null) LoadBanner(); banner.Show(); }
    public void HideBanner() { if (banner != null) banner.Hide(); }

    // ============ INTERSTITIAL ============
    void LoadInterstitial()
    {
        if (interstitial != null) { interstitial.Destroy(); interstitial = null; }
        InterstitialAd.Load(InterstitialId, new AdRequest(), (ad, err) =>
        {
            if (err != null || ad == null)
            {
                Debug.LogWarning("[AdManager] Interstitial yuklenemedi: " + err);
                return;
            }
            interstitial = ad;
            interstitial.OnAdFullScreenContentClosed += LoadInterstitial;
            interstitial.OnAdFullScreenContentFailed += _ => LoadInterstitial();
        });
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            if (onClosed != null)
            {
                Action wrapper = null;
                wrapper = () =>
                {
                    interstitial.OnAdFullScreenContentClosed -= wrapper;
                    onClosed.Invoke();
                };
                interstitial.OnAdFullScreenContentClosed += wrapper;
            }
            interstitial.Show();
        }
        else
        {
            Debug.LogWarning("[AdManager] Interstitial hazir degil");
            LoadInterstitial();
            onClosed?.Invoke(); // reklam yok -> hemen devam et
        }
    }

    // ============ REWARDED ============
    void LoadRewarded()
    {
        if (rewarded != null) { rewarded.Destroy(); rewarded = null; }
        RewardedAd.Load(RewardedId, new AdRequest(), (ad, err) =>
        {
            if (err != null || ad == null)
            {
                Debug.LogWarning("[AdManager] Rewarded yuklenemedi: " + err);
                return;
            }
            rewarded = ad;
            rewarded.OnAdFullScreenContentClosed += LoadRewarded;
            rewarded.OnAdFullScreenContentFailed += _ => LoadRewarded();
        });
    }

    public void ShowRewarded(Action onReward)
    {
        if (rewarded != null && rewarded.CanShowAd())
        {
            rewarded.Show(reward =>
            {
                Debug.Log("[AdManager] Odul kazanildi: " + reward.Amount + " " + reward.Type);
                onReward?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("[AdManager] Rewarded hazir degil");
            LoadRewarded();
        }
    }
}
