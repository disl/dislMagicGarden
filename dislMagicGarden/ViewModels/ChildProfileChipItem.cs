using dislMagicGarden.Models;

namespace dislMagicGarden.ViewModels
{
    /// <summary>
    /// One profile chip on the HomePage.
    /// </summary>
    public record ChildProfileChipItem(ChildProfile Profile, bool IsActive)
    {
        // Name only: an emoji would take a quarter of the limited chip width
        public string DisplayText => Profile.Name;
    }
}
