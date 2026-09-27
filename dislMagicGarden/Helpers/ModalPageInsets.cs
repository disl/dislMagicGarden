namespace dislMagicGarden.Helpers
{
    /// <summary>
    /// Modal pages (Navigation.PushModalAsync) are not inset by the Shell. With Android edge-to-edge
    /// (enforced by targetSdk 35+) they are drawn behind the status bar and the navigation/gesture bar.
    /// Pads the page by exactly the part that is covered by the system bars.
    /// </summary>
    public static class ModalPageInsets
    {
        public static void Apply(ContentPage page)
        {
#if ANDROID
            page.Loaded += (_, _) =>
            {
                if (page.Handler?.PlatformView is Android.Views.View view)
                    view.Post(() => Update(page, view)); // after layout, so the position on screen is known
            };
#endif
        }

#if ANDROID
        private static void Update(ContentPage page, Android.Views.View view)
        {
            try
            {
                var windowInsets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(view);
                if (windowInsets == null)
                    return;

                var bars = windowInsets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars());
                if (bars == null)
                    return;

                var location = new int[2];
                view.GetLocationOnScreen(location);
                var viewTop = location[1];
                var viewBottom = viewTop + view.Height;
                var screenHeight = view.RootView?.Height ?? viewBottom;

                // Only the part really hidden behind the bars (0 if the page is already placed correctly)
                var coveredTop = Math.Max(0, bars.Top - viewTop);
                var coveredBottom = Math.Max(0, viewBottom - (screenHeight - bars.Bottom));

                var density = view.Resources?.DisplayMetrics?.Density ?? 1f;
                page.Padding = new Thickness(
                    page.Padding.Left,
                    coveredTop / density,
                    page.Padding.Right,
                    coveredBottom / density);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ModalPageInsets failed: {ex}");
            }
        }
#endif
    }
}
