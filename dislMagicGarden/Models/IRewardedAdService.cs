namespace dislMagicGarden.Models
{
    public interface IRewardedAdService
    {
        void LoadAd();
        void ShowAd(Action<int> onRewardEarned);
        bool IsLoaded { get; }
    }
}
