using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

public partial class HomePage : FairyBasePage
{
    static bool m_need_for_update = true;

    public HomePage(HomeViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }

    protected async override void OnAppearing()
    {
        if (m_need_for_update)
        {
#if ANDROID
            if (IsPlayCoreApiAvailable())
            {
                await CheckForUpdates();
            }
#endif
            m_need_for_update = false;
        }
    }
        

#if ANDROID
        bool IsPlayCoreApiAvailable()
    {
        try
        {
            var context = Android.App.Application.Context;
            var packageManager = context.PackageManager;
            var playStorePackageName = "com.android.vending";
            var intent = packageManager.GetLaunchIntentForPackage(playStorePackageName);
            return intent != null;
        }
        catch
        {
            return false;
        }


    }


    private async Task CheckForUpdates()
    {
        try
        {
            var updater = new Platforms.Android.InAppUpdater();
            await updater.CheckForUpdatesAsync();
        }
        catch (Exception ex)
        {

            //SentrySdk.CaptureException(ex);
            //await Shell.Current.DisplayAlert("Update Error", ex.Message, "OK");

        }

    }
#endif
}