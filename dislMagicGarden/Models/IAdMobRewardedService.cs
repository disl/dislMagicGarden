namespace dislMagicGarden.Models
{
    public interface IAdMobRewardedService
    {
        Task<bool> ShowRewardedAd();
        void LoadRewardedAd();
        bool IsAdLoaded { get; }
    }
}
