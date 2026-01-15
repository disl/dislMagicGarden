using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace dislMagicGarden.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [ObservableProperty] string appVersion = $"Version {AppInfo.Current.VersionString}";


        [ObservableProperty] FairyTaleTypeOption? selectedFairyTaleType;

        public ObservableCollection<FairyTaleTypeOption> AvailableFairyTaleTypes { get; } = new();

        public ObservableCollection<LanguageOption> AvailableLanguages { get; }
         = new ObservableCollection<LanguageOption>
             {
                new() { Code = "en-US", DisplayName = "English (US)" },
                new() { Code = "de-DE", DisplayName = "Deutsch (DE)" },
                new() { Code = "fr-FR", DisplayName = "Français (FR)" },
                new() { Code = "es-ES", DisplayName = "Español (ES)" },
                new() { Code = "it-IT", DisplayName = "Italiano (IT)" },
                new() { Code = "uk-UA", DisplayName = "Українська (UA)" },
                new() { Code = "ru-RU", DisplayName = "Русский (RU)" },

             };

        private bool _isApplyingLanguage;

        private LanguageOption _selectedLanguage;
        public LanguageOption SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_selectedLanguage?.Code == value.Code)
                    return;

                if (SetProperty(ref _selectedLanguage, value))
                {

                    // verhindert Re-Entry beim Reload / Init
                    if (_isApplyingLanguage)
                        return;

                    _ = ApplyLanguage(value.Code);

                    var culture = new CultureInfo(value.Code);

                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;

                    LocalizationResourceManager.Instance.SetCulture(culture);
                }
            }
        }

        private async Task ApplyLanguage(string lang)
        {
            if (CultureInfo.CurrentUICulture.Name == lang)
                return;

            try
            {
                _isApplyingLanguage = true;

                LanguageService.SetLanguage(lang);
                Preferences.Set("app_language", lang);

                //ReloadFairyTaleTypes();


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Language change failed: " + ex.Message);
            }
            finally
            {
                _isApplyingLanguage = false;
            }
        }

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


        public HomeViewModel()
        {
            Title = "Magic Garden";

            var currentCulture = CultureInfo.CurrentUICulture.Name;

            SelectedLanguage =
                AvailableLanguages.FirstOrDefault(l => l.Code == currentCulture)
                ?? AvailableLanguages.First(l => l.Code.StartsWith("en"));
        }





        [RelayCommand]
        async Task GoToNewStory()
        {
            await Shell.Current.GoToAsync("//FairyTalePage");
        }
    }
}
