// Platforms/Android/FixedAdMobService.cs
using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using dislMagicGarden.Models;
using Java.Lang;
using System;
using Exception = System.Exception;

namespace dislMagicGarden.Platforms.Android
{
    public class AdMobRewardedService : IAdMobRewardedService
    {
        private RewardedAd _rewardedAd;
        private bool _isLoading;
        private TaskCompletionSource<bool> _completionSource;

        private const string TEST_AD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917";

        public bool IsAdLoaded => _rewardedAd != null && !_isLoading;

        public void LoadRewardedAd()
        {
            if (_isLoading || _rewardedAd != null) return;

            _isLoading = true;

            try
            {
                var activity = Platform.CurrentActivity;
                if (activity == null) return;

                // ✅ KORREKT: Für deine Version
                var adRequest = new AdRequest.Builder().Build();

                // ✅ KORREKTER Aufruf für deine API
                RewardedAd.Load(activity, TEST_AD_UNIT_ID, adRequest,
                    new AdLoadCallback(this));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdMob Load Error: {ex.Message}");
                _isLoading = false;
            }
        }

        public async Task<bool> ShowRewardedAd()
        {
            if (!IsAdLoaded)
            {
                LoadRewardedAd();
                await Task.Delay(500);
                if (!IsAdLoaded) return false;
            }

            try
            {
                _completionSource = new TaskCompletionSource<bool>();

                _rewardedAd.FullScreenContentCallback = new FullScreenCallback(this);
                _rewardedAd.Show(Platform.CurrentActivity, new RewardListener(this));

                return await _completionSource.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdMob Show Error: {ex.Message}");
                return false;
            }
        }

        internal void OnAdLoaded(RewardedAd ad)
        {
            _rewardedAd = ad;
            _isLoading = false;
            Console.WriteLine("✅ Ad loaded");
        }

        internal void OnRewardEarned()
        {
            _completionSource?.TrySetResult(true);
            Console.WriteLine("💰 Reward earned");
            _rewardedAd = null;
            LoadRewardedAd();
        }

        internal void OnAdDismissed()
        {
            _completionSource?.TrySetResult(false);
            Console.WriteLine("❎ Ad dismissed");
            _rewardedAd = null;
            LoadRewardedAd();
        }
    }

    // ✅ KORREKTER Callback für deine Version
    internal class AdLoadCallback : RewardedAdLoadCallback
    {
        private readonly AdMobRewardedService _service;

        public AdLoadCallback(AdMobRewardedService service)
        {
            _service = service;
        }

        // ✅ KORREKT: Diese Methode überschreiben
        public override void OnAdLoaded(Java.Lang.Object ad)
        {
            if (ad is RewardedAd rewardedAd)
            {
                _service.OnAdLoaded(rewardedAd);
            }
        }

        public override void OnAdFailedToLoad(LoadAdError error)
        {
            Console.WriteLine($"Ad load failed: {error?.Message}");
        }
    }

    internal class FullScreenCallback : FullScreenContentCallback
    {
        private readonly AdMobRewardedService _service;

        public FullScreenCallback(AdMobRewardedService service)
        {
            _service = service;
        }

        public override void OnAdDismissedFullScreenContent()
        {
            _service.OnAdDismissed();
        }

        public override void OnAdFailedToShowFullScreenContent(AdError error)
        {
            _service.OnAdDismissed();
        }
    }

    internal class RewardListener : Java.Lang.Object, IOnUserEarnedRewardListener
    {
        private readonly AdMobRewardedService _service;

        public RewardListener(AdMobRewardedService service)
        {
            _service = service;
        }

        public void OnUserEarnedReward(IRewardItem reward)
        {
            _service.OnRewardEarned();
        }
    }
}