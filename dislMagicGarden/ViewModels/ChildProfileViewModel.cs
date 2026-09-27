using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using dislMagicGarden.Services;
using System.Diagnostics;

namespace dislMagicGarden.ViewModels
{
    /// <summary>
    /// Form for one child profile (name, gender, description) incl. the mandatory privacy consent.
    /// Opened with query parameter "id" to edit an existing profile, without to create a new one.
    /// </summary>
    public partial class ChildProfileViewModel : BaseViewModel, IQueryAttributable
    {
        public const string QueryId = "id";

        private readonly ChildProfileService _childProfile;
        private Guid _profileId = Guid.NewGuid();

        public int MaxNameLength => ChildProfileService.MaxNameLength;
        public int MaxDescriptionLength => ChildProfileService.MaxDescriptionLength;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ValidationMessage))]
        [NotifyPropertyChangedFor(nameof(HasValidationMessage))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _name = string.Empty;

        [ObservableProperty]
        private GenderOption _gender = GenderOption.Neutral;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CharCountText))]
        [NotifyPropertyChangedFor(nameof(ValidationMessage))]
        [NotifyPropertyChangedFor(nameof(HasValidationMessage))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _description = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ValidationMessage))]
        [NotifyPropertyChangedFor(nameof(HasValidationMessage))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool _hasConsent;

        [ObservableProperty]
        private bool _isExistingProfile;

        public string CharCountText => $"{Description?.Length ?? 0} / {MaxDescriptionLength}";

        /// <summary>
        /// Why the profile cannot be saved yet (empty if it can).
        /// </summary>
        public string ValidationMessage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return LocalizationResourceManager.Instance["Child_profile_name_required"];

                if (!string.IsNullOrWhiteSpace(Description) && !HasConsent)
                    return LocalizationResourceManager.Instance["Child_profile_consent_required"];

                return string.Empty;
            }
        }

        public bool HasValidationMessage => !string.IsNullOrEmpty(ValidationMessage);

        public ChildProfileViewModel(ChildProfileService childProfile)
        {
            _childProfile = childProfile;
            Title = LocalizationResourceManager.Instance["Child_description"];
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(QueryId, out var value)
                && Guid.TryParse(value?.ToString(), out var id)
                && _childProfile.GetProfile(id) is { } profile)
            {
                _profileId = profile.Id;
                Name = profile.Name;
                Gender = profile.Gender;
                Description = profile.Description;
                HasConsent = profile.HasConsent;
                IsExistingProfile = true;
            }
        }

        private bool CanSave() => !HasValidationMessage;

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            try
            {
                var profile = new ChildProfile(_profileId, Name, Gender, Description, HasConsent);
                if (!_childProfile.Save(profile))
                {
                    // Only reachable if the form state and the service rules diverge (e.g. max. profiles)
                    await Shell.Current.DisplayAlert(Properties.Resources.Error, ValidationMessage, "OK");
                    return;
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Saving child profile failed: {ex}");
                await Shell.Current.DisplayAlert(Properties.Resources.Error, ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            try
            {
                var loc = LocalizationResourceManager.Instance;
                var confirmed = await Shell.Current.DisplayAlert(
                    loc["Child_profile_delete"],
                    string.Format(loc["Child_profile_delete_confirm"], Name),
                    loc["Child_profile_delete_yes"],
                    loc["Child_profile_cancel"]);

                if (!confirmed)
                    return;

                _childProfile.Delete(_profileId);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Deleting child profile failed: {ex}");
                await Shell.Current.DisplayAlert(Properties.Resources.Error, ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Leaving child profile failed: {ex}");
            }
        }
    }
}
