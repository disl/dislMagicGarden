using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using dislMagicGarden.Models;
using Microsoft.Maui.Controls.Shapes;
using static AndroidX.Room.FtsOptions;

[assembly: Dependency(typeof(dislMagicGarden.Platforms.Android.RewardedAdService))]
namespace dislMagicGarden.Platforms.Android
{
    public class RewardedAdService : IRewardedAdService
    {
        private RewardedAd _rewardedAd;
        // Test-ID für Rewarded Ads:
        private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";

        public bool IsLoaded => _rewardedAd != null;

        public void LoadAd()
        {
            var adRequest = new AdRequest.Builder().Build();
            RewardedAd.Load(Platform.CurrentActivity, _adUnitId, adRequest, new MyRewardedAdLoadCallback(ad => _rewardedAd = ad));
        }

        public void ShowAd(Action<int> onRewardEarned)
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Show(Platform.CurrentActivity, new OnUserEarnedRewardListener(rewardItem =>
                {
                    // Belohnung an MAUI zurückgeben
                    onRewardEarned?.Invoke(rewardItem.Amount);
                    _rewardedAd = null;
                    LoadAd(); // Nächstes Video im Hintergrund laden
                }));
            }
        }
    }

    // Callback für den Ladevorgang
    public class MyRewardedAdLoadCallback : RewardedAdLoadCallback
    {
        private readonly Action<RewardedAd> _onLoaded;

        public MyRewardedAdLoadCallback(Action<RewardedAd> onLoaded)
        {
            _onLoaded = onLoaded;
        }

        // WICHTIG: Die Signatur muss exakt so aussehen
        //public override void OnAdLoaded(RewardedAd rewardedAd)
        //{
        //    base.OnAdLoaded(rewardedAd);
        //    _onLoaded?.Invoke(rewardedAd);
        //}

        /// <summary>
        /// Code 0 (Internal Error): Oft Konfigurationsfehler oder Internetprobleme.
        /// Code 2 (Network Error): Keine Internetverbindung oder Firewall/Emulator-Problem.
        /// Code 3 (No Fill): Das passiert bei Test-IDs selten, heißt aber eigentlich: "Keine Werbung verfügbar".
        /// </summary>
        /// <param name="error"></param>
        public override void OnAdFailedToLoad(LoadAdError error)
        {
            base.OnAdFailedToLoad(error);
            // Optional: Logge den Fehler, um zu sehen, warum keine Werbung kommt
            System.Diagnostics.Debug.WriteLine($"AdMob Error: {error.Message}");
        }
    }

    // Callback für die Belohnung
    public class OnUserEarnedRewardListener : Java.Lang.Object, IOnUserEarnedRewardListener
    {
        private Action<IRewardItem> _onReward;
        public OnUserEarnedRewardListener(Action<IRewardItem> onReward) => _onReward = onReward;
        public void OnUserEarnedReward(IRewardItem rewardItem) => _onReward?.Invoke(rewardItem);
    }
}
