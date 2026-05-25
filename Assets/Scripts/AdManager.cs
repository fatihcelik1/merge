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

    // Google'in resmi test ad unit ID'leri (asla productionda kullanma)
    const string BannerId       = "ca-app-pub-3940256099942544/6300978111";
    const string InterstitialId = "ca-app-pub-3940256099942544/1033173712";
    const string RewardedId     = "ca-app-pub-3940256099942544/5224354917";

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
