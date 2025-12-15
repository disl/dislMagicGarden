using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using System.Collections.ObjectModel;

namespace dislMagicGarden.ViewModels
{
    public partial class FairyTaleTypeViewModel : ObservableObject
    {
        public ObservableCollection<FairyTaleTypeOption> Types { get; }

        [ObservableProperty]
        private FairyTaleTypeOption? selectedType;

        public bool CanContinue => SelectedType != null;

        public IRelayCommand ContinueCommand { get; }

        public FairyTaleTypeViewModel()
        {
            Types = new ObservableCollection<FairyTaleTypeOption>();

            ContinueCommand = new RelayCommand(OnContinue, () => CanContinue);
        }

        partial void OnSelectedTypeChanged(FairyTaleTypeOption? value)
        {
            ContinueCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanContinue));
        }

        private async void OnContinue()
        {
            if (SelectedType == null)
                return;

            // Beispiel: Auswahl speichern
            Preferences.Set("fairytale_type", SelectedType.Type.ToString());

            // Navigation zur nächsten Page
            await Shell.Current.GoToAsync("FairyTalePage");
        }
    }
}
