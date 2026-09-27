using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using dislMagicGarden.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace dislMagicGarden.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        // Child profiles (switchable chips, theme follows the gender of the active profile)
        private readonly ChildProfileService _childProfile;

        public ObservableCollection<ChildProfileChipItem> ProfileChips { get; } = new();

        [ObservableProperty]
        bool canAddProfile;

        // Without any profile a bare "+" is not self-explanatory
        [ObservableProperty]
        string addProfileText = "＋";

        /// <summary>
        /// Rebuilds the profile chips and applies the theme of the active profile.
        /// Called on construction and whenever the HomePage appears (e.g. after editing a profile).
        /// </summary>
        public void RefreshProfiles()
        {
            var activeId = _childProfile.ActiveProfile?.Id;

            ProfileChips.Clear();
            foreach (var profile in _childProfile.Profiles)
                ProfileChips.Add(new ChildProfileChipItem(profile, profile.Id == activeId));

            CanAddProfile = _childProfile.CanAddProfile;
            AddProfileText = ProfileChips.Count == 0
                ? $"＋ {LocalizationResourceManager.Instance["Child_profile_add"]}"
                : "＋";
            ApplyTheme(_childProfile.Gender == GenderOption.Male);
        }

        [RelayCommand]
        async Task SelectProfile(ChildProfileChipItem item)
        {
            // Tap on the active chip edits it, tap on another chip switches the profile
            if (item.IsActive)
            {
                await OpenProfileAsync(item.Profile.Id);
                return;
            }

            _childProfile.SetActive(item.Profile.Id);
            RefreshProfiles();
        }

        [RelayCommand]
        async Task AddProfile() => await OpenProfileAsync(null);

        [RelayCommand]
        async Task EditActiveProfile() => await OpenProfileAsync(_childProfile.ActiveProfile?.Id);

        private static async Task OpenProfileAsync(Guid? id)
        {
            try
            {
                var route = id.HasValue
                    ? $"{nameof(ChildProfilePage)}?{ChildProfileViewModel.QueryId}={id.Value}"
                    : nameof(ChildProfilePage);
                await Shell.Current.GoToAsync(route);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Opening child profile failed: {ex}");
            }
        }


        // Pictures
        [ObservableProperty] string headerTaleImage = "header_fairy_4.webp";

        private void ApplyTheme(bool useBlue)
        {
            var res = Application.Current.Resources;

            if (useBlue)
            {
                res["ThemePrimaryColor"] = Color.FromArgb("#4A90E2");
                res["ThemeBackgroundColor"] = Color.FromArgb("#D1E9FF");
                // Du kannst hier auch die Hintergrundfarbe der HomePage ändern:
                res["CottonCandy"] = res["SkyAbyss"];
                res["ThemeLabelColor"] = Color.FromArgb("#190649"); // Dunkles Blau (MidnightBlue)
                res["DynamicPageBackground"] = Color.FromArgb("#CFEAFF");
                HeaderTaleImage = "header_fairy_for_man.webp";
                

            }
            else
            {
                res["ThemePrimaryColor"] = Color.FromArgb("#FF9ECD");
                res["ThemeBackgroundColor"] = Color.FromArgb("#FFE2F1");
                res["CottonCandy"] = Color.FromArgb("#FFE2F1"); // Original CottonCandy
                res["ThemeLabelColor"] = Color.FromArgb("#4A4A4A"); // Klassisch Grau (MagicBlackSoft)
                res["DynamicPageBackground"] = Color.FromArgb("#FFE2F1");
                HeaderTaleImage = "header_fairy_4.webp";
                
            }

            OnPropertyChanged(nameof(HeaderTaleImage));
        }




        [ObservableProperty] FairyTaleTypeOption? selectedFairyTaleType;

        public ObservableCollection<FairyTaleTypeOption> AvailableFairyTaleTypes { get; } = new();

        //public void ReloadFairyTaleTypes()
        //{
        //    var selectedType = SelectedFairyTaleType?.Type;

        //    AvailableFairyTaleTypes.Clear();
        //    foreach (var item in FairyTaleTypes.Create())
        //        AvailableFairyTaleTypes.Add(item);

        //    // Auswahl wiederherstellen
        //    if (selectedType != null)
        //        SelectedFairyTaleType =
        //            AvailableFairyTaleTypes.FirstOrDefault(x => x.Type == selectedType);
        //    else
        //        SelectedFairyTaleType =
        //            AvailableFairyTaleTypes.FirstOrDefault();
        //}


        public HomeViewModel(ChildProfileService childProfile)
        {
            Title = "Magic Garden";

            _childProfile = childProfile;

            // Language is chosen in the settings (default: device language, see App.xaml.cs)
            RefreshProfiles();
        }





        [RelayCommand]
        async Task GoToNewStory()
        {
            await Shell.Current.GoToAsync("//FairyTalePage");
        }
    }
}
