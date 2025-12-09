using dislMagicGarden.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiLocale = Microsoft.Maui.Media.Locale;

namespace dislMagicGarden.ViewModels
{
    public class FairyTaleResultViewModel : BaseViewModel
    {
        public FairyTaleModel FairyTale { get; }
        private readonly Action _closeAction;

        // Verfügbare Stimmen
        public ObservableCollection<LocaleWrapper> AvailableVoices { get; } = new();
        private LocaleWrapper _selectedVoice;
        public LocaleWrapper SelectedVoice
        {
            get => _selectedVoice;
            set => SetProperty(ref _selectedVoice, value);
        }

       


        public ICommand SpeakStoryCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand CloseCommand { get; }

        public async Task LoadVoicesAsync()
        {
            var locales = await TextToSpeech.GetLocalesAsync(); // liefert MAUI-Locales!

            AvailableVoices.Clear();
            foreach (var locale in locales.Where(v => v.Language.StartsWith("de")))
            {
                AvailableVoices.Add(new LocaleWrapper { Locale = locale });
            }

            SelectedVoice = AvailableVoices
                .FirstOrDefault(v => v.Locale.Language == "de")
                ?? AvailableVoices.FirstOrDefault();
        }

        public FairyTaleResultViewModel(FairyTaleModel fairyTale, Action closeAction)
        {
            FairyTale = fairyTale;
            _closeAction = closeAction;

            SpeakStoryCommand = new Command(async () =>
            {
                var options = new SpeechOptions
                {
                    Locale = SelectedVoice?.Locale, // 💡 jetzt eindeutig
                    Pitch = 1.15f,                   // etwas höher = märchenhaft
                    Volume = 1.0f
                };

                await TextToSpeech.SpeakAsync(FairyTale.Story, options);
            });

            ShareCommand = new Command(async () =>
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Title = "Märchen teilen",
                    Text = FairyTale.Story
                });
            });

            CloseCommand = new Command(_closeAction);
        }
    }

}
