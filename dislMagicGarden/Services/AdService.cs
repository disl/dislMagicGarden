using Plugin.MauiMTAdmob;
using System.Diagnostics;

namespace dislMagicGarden.Services
{
    public class AdService : IDisposable
    {
        // Ad Unit IDs
        private const string TEST_INTERSTITIAL_ID = "ca-app-pub-3940256099942544/1033173712";
        private const string TEST_REWARDED_ID = "ca-app-pub-3940256099942544/5224354917";
        private const string TEST_BANNER_ID = "ca-app-pub-3940256099942544/6300978111";

        // Eigene Status-Tracking Variablen
        private bool _isInterstitialLoaded = false;
        private bool _isRewardedLoaded = false;
        private bool _isInterstitialLoading = false;
        private bool _isRewardedLoading = false;

        private string _interstitialId = TEST_INTERSTITIAL_ID;
        private string _rewardedId = TEST_REWARDED_ID;
        private string _bannerId = TEST_BANNER_ID;

        private DateTime _lastInterstitialLoadAttempt = DateTime.MinValue;
        private DateTime _lastRewardedLoadAttempt = DateTime.MinValue;

        // Events
        public event EventHandler<bool> OnInterstitialLoadChanged;
        public event EventHandler<bool> OnRewardedLoadChanged;
        public event EventHandler<string> OnAdStatusChanged;
        public event EventHandler<bool> OnRewardedAdCompleted;

        // Task Completion Sources
        private TaskCompletionSource<bool> _rewardedAdCompletionSource;

        public AdService()
        {
            Debug.WriteLine("[AdService] Initialisiere...");
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (CrossMauiMTAdmob.Current == null)
                {
                    Debug.WriteLine("[AdService] ERROR: Plugin nicht verfügbar!");
                    return;
                }

                // Interstitial Events
                CrossMauiMTAdmob.Current.OnInterstitialLoaded += OnInterstitialLoadedHandler;
                CrossMauiMTAdmob.Current.OnInterstitialFailedToLoad += OnInterstitialFailedHandler;
                CrossMauiMTAdmob.Current.OnInterstitialOpened += OnInterstitialOpenedHandler;
                CrossMauiMTAdmob.Current.OnInterstitialClosed += OnInterstitialClosedHandler;
                CrossMauiMTAdmob.Current.OnInterstitialFailedToShow += OnInterstitialFailedToShowHandler;

                // Rewarded Events
                CrossMauiMTAdmob.Current.OnRewardedLoaded += OnRewardedLoadedHandler;
                CrossMauiMTAdmob.Current.OnRewardedFailedToLoad += OnRewardedFailedHandler;
                CrossMauiMTAdmob.Current.OnRewardedOpened += OnRewardedOpenedHandler;
                CrossMauiMTAdmob.Current.OnRewardedClosed += OnRewardedClosedHandler;
                CrossMauiMTAdmob.Current.OnRewardedFailedToShow += OnRewardedFailedToShowHandler;
                //CrossMauiMTAdmob.Current.OnRewardedEarned += OnRewardedEarnedHandler;

                // Banner Events
                //CrossMauiMTAdmob.Current.OnBannerLoaded += OnBannerLoadedHandler;
                //CrossMauiMTAdmob.Current.OnBannerFailedToLoad += OnBannerFailedHandler;

                // Initiale Ads laden (mit Verzögerung)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000); // 3 Sekunden nach Start
                    await LoadInitialAds();
                });

                Debug.WriteLine("[AdService] ✅ Erfolgreich initialisiert");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] Initialisierungsfehler: {ex.Message}");
            }
        }

        #region EVENT HANDLERS

        // Interstitial Event Handlers
        private void OnInterstitialLoadedHandler(object sender, EventArgs e)
        {
            _isInterstitialLoaded = true;
            _isInterstitialLoading = false;
            Debug.WriteLine("[AdService] ✅ Interstitial geladen (Event)");
            OnInterstitialLoadChanged?.Invoke(this, true);
            OnAdStatusChanged?.Invoke(this, "Interstitial geladen");
        }

        private void OnInterstitialFailedHandler(object sender, EventArgs e)
        {
            _isInterstitialLoaded = false;
            _isInterstitialLoading = false;
            Debug.WriteLine("[AdService] ❌ Interstitial Load Fehler");
            OnAdStatusChanged?.Invoke(this, "Interstitial Fehler");
        }

        private void OnInterstitialOpenedHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] 📱 Interstitial geöffnet");
            OnAdStatusChanged?.Invoke(this, "Interstitial geöffnet");
        }

        private void OnInterstitialClosedHandler(object sender, EventArgs e)
        {
            _isInterstitialLoaded = false;
            Debug.WriteLine("[AdService] 📴 Interstitial geschlossen");
            OnAdStatusChanged?.Invoke(this, "Interstitial geschlossen");

            // Automatisch neu laden
            _ = LoadInterstitialAsync();
        }

        private void OnInterstitialFailedToShowHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] ❌ Interstitial Show Fehler");
            _isInterstitialLoaded = false;
        }

        // Rewarded Event Handlers
        private void OnRewardedLoadedHandler(object sender, EventArgs e)
        {
            _isRewardedLoaded = true;
            _isRewardedLoading = false;
            Debug.WriteLine("[AdService] ✅ Rewarded geladen (Event)");
            OnRewardedLoadChanged?.Invoke(this, true);
            OnAdStatusChanged?.Invoke(this, "Rewarded geladen");
        }

        private void OnRewardedFailedHandler(object sender, EventArgs e)
        {
            _isRewardedLoaded = false;
            _isRewardedLoading = false;
            Debug.WriteLine("[AdService] ❌ Rewarded Load Fehler");
            OnAdStatusChanged?.Invoke(this, "Rewarded Fehler");

            // Task Completion Source auf false setzen
            _rewardedAdCompletionSource?.TrySetResult(false);
        }

        private void OnRewardedOpenedHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] 📱 Rewarded geöffnet");
            OnAdStatusChanged?.Invoke(this, "Rewarded geöffnet");
        }

        private void OnRewardedClosedHandler(object sender, EventArgs e)
        {
            _isRewardedLoaded = false;
            Debug.WriteLine("[AdService] 📴 Rewarded geschlossen");
            OnAdStatusChanged?.Invoke(this, "Rewarded geschlossen");

            // Task Completion Source auf false setzen (falls noch nicht gesetzt)
            _rewardedAdCompletionSource?.TrySetResult(false);

            // Automatisch neu laden
            _ = LoadRewardedAsync();
        }

        private void OnRewardedFailedToShowHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] ❌ Rewarded Show Fehler");
            _isRewardedLoaded = false;
            _rewardedAdCompletionSource?.TrySetResult(false);
        }

        private void OnRewardedEarnedHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] 🎉 Rewarded verdient!");
            OnRewardedAdCompleted?.Invoke(this, true);
            OnAdStatusChanged?.Invoke(this, "Reward verdient!");

            // Task Completion Source auf true setzen
            _rewardedAdCompletionSource?.TrySetResult(true);
        }

        // Banner Event Handlers
        private void OnBannerLoadedHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] ✅ Banner geladen");
        }

        private void OnBannerFailedHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("[AdService] ❌ Banner Fehler");
        }

        #endregion

        #region INTERSTITIAL ADS

        /// <summary>
        /// Lädt eine Interstitial Ad
        /// </summary>
        public async Task<bool> LoadInterstitialAsync()
        {
            if (_isInterstitialLoading) return false;

            // Minimaler Abstand zwischen Load-Versuchen
            var timeSinceLastAttempt = DateTime.Now - _lastInterstitialLoadAttempt;
            if (timeSinceLastAttempt.TotalSeconds < 10)
            {
                Debug.WriteLine($"[AdService] Zu früh für neues Laden: {timeSinceLastAttempt.TotalSeconds:F1}s");
                return _isInterstitialLoaded;
            }

            _isInterstitialLoading = true;
            _lastInterstitialLoadAttempt = DateTime.Now;

            try
            {
                Debug.WriteLine($"[AdService] Starte Interstitial Load...");

                // Status zurücksetzen
                _isInterstitialLoaded = false;
                OnAdStatusChanged?.Invoke(this, "Lade Interstitial...");

                // Plugin Load aufrufen
                 CrossMauiMTAdmob.Current.LoadInterstitial(_interstitialId);

                // Auf Event warten (mit Timeout)
                var loaded = await WaitForInterstitialLoaded(5000); // 5 Sekunden Timeout

                Debug.WriteLine($"[AdService] Interstitial Load Ergebnis: {loaded}");
                return loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] LoadInterstitialAsync Fehler: {ex.Message}");
                _isInterstitialLoaded = false;
                _isInterstitialLoading = false;
                return false;
            }
        }

        /// <summary>
        /// Eigene IsLoaded Methode
        /// </summary>
        public bool IsInterstitialLoaded()
        {
            return _isInterstitialLoaded;
        }

        /// <summary>
        /// Zeigt Interstitial wenn geladen
        /// </summary>
        public bool ShowInterstitial()
        {
            if (!_isInterstitialLoaded)
            {
                Debug.WriteLine("[AdService] Interstitial nicht geladen");
                return false;
            }

            try
            {
                Debug.WriteLine("[AdService] Zeige Interstitial...");
                CrossMauiMTAdmob.Current.ShowInterstitial();

                // Status sofort zurücksetzen
                _isInterstitialLoaded = false;
                OnInterstitialLoadChanged?.Invoke(this, false);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] ShowInterstitial Fehler: {ex.Message}");
                _isInterstitialLoaded = false;
                return false;
            }
        }

        /// <summary>
        /// Lädt und zeigt Interstitial
        /// </summary>
        public async Task<bool> LoadAndShowInterstitial()
        {
            Debug.WriteLine("[AdService] LoadAndShowInterstitial gestartet...");

            // 1. Laden
            bool loaded = await LoadInterstitialAsync();

            if (!loaded)
            {
                Debug.WriteLine("[AdService] Konnte Interstitial nicht laden");
                return false;
            }

            // 2. Kurze Pause
            await Task.Delay(1000);

            // 3. Prüfen und anzeigen
            if (_isInterstitialLoaded)
            {
                return ShowInterstitial();
            }

            return false;
        }

        /// <summary>
        /// Versucht Interstitial zu zeigen (mit automatischem Laden)
        /// </summary>
        public async Task<bool> TryShowInterstitial()
        {
            // Wenn bereits geladen, direkt zeigen
            if (_isInterstitialLoaded)
            {
                return ShowInterstitial();
            }

            // Sonst laden und zeigen
            return await LoadAndShowInterstitial();
        }

        private async Task<bool> WaitForInterstitialLoaded(int timeoutMs)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                if (_isInterstitialLoaded)
                {
                    stopwatch.Stop();
                    return true;
                }

                await Task.Delay(100);
            }

            stopwatch.Stop();
            Debug.WriteLine($"[AdService] Timeout beim Warten auf Interstitial");
            _isInterstitialLoading = false;
            return false;
        }

        #endregion

        #region REWARDED ADS

        /// <summary>
        /// Lädt eine Rewarded Ad
        /// </summary>
        public async Task<bool> LoadRewardedAsync()
        {
            if (_isRewardedLoading) return false;

            var timeSinceLastAttempt = DateTime.Now - _lastRewardedLoadAttempt;
            if (timeSinceLastAttempt.TotalSeconds < 10)
            {
                return _isRewardedLoaded;
            }

            _isRewardedLoading = true;
            _lastRewardedLoadAttempt = DateTime.Now;

            try
            {
                Debug.WriteLine($"[AdService] Starte Rewarded Load...");

                _isRewardedLoaded = false;
                OnAdStatusChanged?.Invoke(this, "Lade Rewarded...");

                CrossMauiMTAdmob.Current.LoadRewarded(_rewardedId);

                var loaded = await WaitForRewardedLoaded(15000);

                Debug.WriteLine($"[AdService] Rewarded Load Ergebnis: {loaded}");
                return loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] LoadRewardedAsync Fehler: {ex.Message}");
                _isRewardedLoaded = false;
                _isRewardedLoading = false;
                return false;
            }
        }

        /// <summary>
        /// Eigene IsRewardedLoaded Methode
        /// </summary>
        public bool IsRewardedLoaded()
        {
            return _isRewardedLoaded;
        }

        /// <summary>
        /// Zeigt Rewarded Ad
        /// </summary>
        public async Task<bool> ShowRewardedAd()
        {
            // Task Completion Source erstellen
            _rewardedAdCompletionSource = new TaskCompletionSource<bool>();

            // Prüfen ob Ad geladen ist
            if (!_isRewardedLoaded)
            {
                Debug.WriteLine("[AdService] Rewarded nicht geladen, lade zuerst...");
                bool loaded = await LoadRewardedAsync();
                if (!loaded)
                {
                    _rewardedAdCompletionSource.TrySetResult(false);
                    return false;
                }
            }

            try
            {
                Debug.WriteLine("[AdService] Zeige Rewarded...");
                CrossMauiMTAdmob.Current.ShowRewarded();

                // Status zurücksetzen
                _isRewardedLoaded = false;
                OnRewardedLoadChanged?.Invoke(this, false);

                // Auf Ergebnis warten (mit Timeout)
                var timeoutTask = Task.Delay(120000); // 2 Minuten Timeout
                var completedTask = await Task.WhenAny(_rewardedAdCompletionSource.Task, timeoutTask);

                if (completedTask == _rewardedAdCompletionSource.Task)
                {
                    bool result = await _rewardedAdCompletionSource.Task;
                    Debug.WriteLine($"[AdService] Rewarded abgeschlossen: {result}");
                    return result;
                }
                else
                {
                    Debug.WriteLine("[AdService] Rewarded Timeout");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] ShowRewardedAd Fehler: {ex.Message}");
                _rewardedAdCompletionSource.TrySetResult(false);
                return false;
            }
            finally
            {
                // Sofort neu laden
                _ = LoadRewardedAsync();
            }
        }

        private async Task<bool> WaitForRewardedLoaded(int timeoutMs)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                if (_isRewardedLoaded)
                {
                    stopwatch.Stop();
                    return true;
                }

                await Task.Delay(100);
            }

            stopwatch.Stop();
            Debug.WriteLine($"[AdService] Timeout beim Warten auf Rewarded");
            _isRewardedLoading = false;
            return false;
        }

        #endregion

        #region BANNER ADS




        /// <summary>
        /// Erstellt eine Banner Ad View
        /// </summary>
        //public MTAdmobView CreateBannerAd(AdSize adSize = AdSize.Banner, AdPosition position = AdPosition.Bottom)
        //{
        //    try
        //    {
        //        var banner = new MTAdmobView
        //        {
        //            AdUnitId = _bannerId,
        //            AdSize = adSize,
        //            VerticalOptions = position == AdPosition.Bottom ? LayoutOptions.End : LayoutOptions.Start,
        //            HorizontalOptions = LayoutOptions.Fill,
        //            HeightRequest = adSize == AdSize.Banner ? 50 : 100
        //        };

        //        return banner;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"[AdService] CreateBannerAd Fehler: {ex.Message}");
        //        return null;
        //    }
        //}


        public enum AdPosition
        {
            Top,
            Bottom
        }

        #endregion

        #region HELPER METHODS

        /// <summary>
        /// Lädt initial alle Ads
        /// </summary>
        private async Task LoadInitialAds()
        {
            try
            {
                Debug.WriteLine("[AdService] Starte Initiales Preloading...");

                // Beide Ads parallel laden
                await Task.WhenAll(
                    LoadInterstitialAsync(),
                    LoadRewardedAsync()
                );

                Debug.WriteLine($"[AdService] Preloading abgeschlossen");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] LoadInitialAds Fehler: {ex.Message}");
            }
        }

        /// <summary>
        /// Debug-Info
        /// </summary>
        public string GetDebugInfo()
        {
            return $"""
            === AD SERVICE DEBUG ===
            Plugin verfügbar: {CrossMauiMTAdmob.Current != null}
            Plugin.IsInterstitialLoaded(): {CrossMauiMTAdmob.Current?.IsInterstitialLoaded() ?? false}
            Eigenes Tracking (Interstitial): {_isInterstitialLoaded}
            Eigenes Tracking (Rewarded): {_isRewardedLoaded}
            Wird geladen (Interstitial): {_isInterstitialLoading}
            Wird geladen (Rewarded): {_isRewardedLoading}
            ========================
            """;
        }

        /// <summary>
        /// Direkter Workaround
        /// </summary>
        public bool CheckPluginInterstitialLoaded()
        {
            try
            {
                bool pluginResult = CrossMauiMTAdmob.Current.IsInterstitialLoaded();
                return pluginResult || _isInterstitialLoaded;
            }
            catch
            {
                return _isInterstitialLoaded;
            }
        }

        /// <summary>
        /// Setzt Production IDs
        /// </summary>
        public void SetProductionIds(string interstitialId, string rewardedId, string bannerId)
        {
            _interstitialId = interstitialId;
            _rewardedId = rewardedId;
            _bannerId = bannerId;
            Debug.WriteLine("[AdService] Auf Production IDs umgestellt");
        }

        /// <summary>
        /// Test-Methode
        /// </summary>
        public async Task TestAdSystem()
        {
            Debug.WriteLine("=== TEST AD SYSTEM ===");

            // Test Interstitial
            Debug.WriteLine("1. Teste Interstitial...");
            bool interstitialLoaded = await LoadInterstitialAsync();
            Debug.WriteLine($"   Interstitial geladen: {interstitialLoaded}");

            if (interstitialLoaded)
            {
                await Task.Delay(1000);
                bool shown = ShowInterstitial();
                Debug.WriteLine($"   Interstitial gezeigt: {shown}");
            }

            await Task.Delay(2000);

            // Test Rewarded
            Debug.WriteLine("2. Teste Rewarded...");
            bool rewardedLoaded = await LoadRewardedAsync();
            Debug.WriteLine($"   Rewarded geladen: {rewardedLoaded}");

            Debug.WriteLine("=== TEST ENDE ===");
        }

        #endregion

        #region DISPOSE

        public void Dispose()
        {
            try
            {
                // Event Handler entfernen
                if (CrossMauiMTAdmob.Current != null)
                {
                    CrossMauiMTAdmob.Current.OnInterstitialLoaded -= OnInterstitialLoadedHandler;
                    CrossMauiMTAdmob.Current.OnInterstitialFailedToLoad -= OnInterstitialFailedHandler;
                    CrossMauiMTAdmob.Current.OnInterstitialOpened -= OnInterstitialOpenedHandler;
                    CrossMauiMTAdmob.Current.OnInterstitialClosed -= OnInterstitialClosedHandler;
                    CrossMauiMTAdmob.Current.OnInterstitialFailedToShow -= OnInterstitialFailedToShowHandler;

                    CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnRewardedLoadedHandler;
                    CrossMauiMTAdmob.Current.OnRewardedFailedToLoad -= OnRewardedFailedHandler;
                    CrossMauiMTAdmob.Current.OnRewardedOpened -= OnRewardedOpenedHandler;
                    CrossMauiMTAdmob.Current.OnRewardedClosed -= OnRewardedClosedHandler;
                    CrossMauiMTAdmob.Current.OnRewardedFailedToShow -= OnRewardedFailedToShowHandler;
                    //CrossMauiMTAdmob.Current.OnRewardedEarned -= OnRewardedEarnedHandler;

                    //CrossMauiMTAdmob.Current.OnBannerLoaded -= OnBannerLoadedHandler;
                    //CrossMauiMTAdmob.Current.OnBannerFailedToLoad -= OnBannerFailedHandler;
                }

                // Task Completion Source aufräumen
                _rewardedAdCompletionSource?.TrySetResult(false);

                Debug.WriteLine("[AdService] Dispose abgeschlossen");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AdService] Dispose Fehler: {ex.Message}");
            }
        }

        #endregion
    }
}