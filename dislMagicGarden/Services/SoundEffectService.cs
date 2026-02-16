using Plugin.Maui.Audio;

namespace dislMagicGarden.Services
{
    public class SoundEffectService
    {
        private readonly IAudioManager _audioManager;
        private IAudioPlayer _musicPlayer; // Für Hintergrundmusik
                                           // Aktuell eingestellte Sprache (z.B. "de" oder "en")
        public string CurrentLanguage { get; set; } = "de";

        // Struktur: Sprache -> Keyword -> Dateiname
        private readonly Dictionary<string, Dictionary<string, string>> _languageSoundMaps;

        public SoundEffectService(IAudioManager audioManager)
        {
            _audioManager = audioManager;

            // Initialisierung derMappings
            _languageSoundMaps = new Dictionary<string, Dictionary<string, string>>
            {
                // --- DEUTSCH ---
                ["de"] = new Dictionary<string, string>
            {
                { "tür", "door_creak.mp3" },
                { "knarrte", "door_creak.mp3" },
                { "türschwelle", "door_creak.mp3" },
                { "wald", "forest_ambience.mp3" },
                { "baum", "forest_ambience.mp3" },
                { "schritt", "footsteps.mp3" },
                { "lief", "footsteps_running.mp3" },
                { "donner", "thunder.mp3" },
                { "sturm", "storm_wind.mp3" },
                { "zauber", "magic_sparkle.mp3" },
                { "hexe", "witch_cackle.mp3" },
                { "drache", "dragon_roar.mp3" },
                { "meer", "ocean_waves.mp3" },
                { "wasser", "water_splash.mp3" }
            },

                // --- ENGLISCH ---
                ["en"] = new Dictionary<string, string>
            {
                { "door", "door_creak.mp3" },
                { "creaked", "door_creak.mp3" },
                { "forest", "forest_ambience.mp3" },
                { "tree", "forest_ambience.mp3" },
                { "step", "footsteps.mp3" },
                { "walked", "footsteps.mp3" },
                { "ran", "footsteps_running.mp3" },
                { "thunder", "thunder.mp3" },
                { "storm", "storm_wind.mp3" },
                { "magic", "magic_sparkle.mp3" },
                { "witch", "witch_cackle.mp3" },
                { "dragon", "dragon_roar.mp3" },
                { "sea", "ocean_waves.mp3" },
                { "ocean", "ocean_waves.mp3" },
                { "water", "water_splash.mp3" }
            },

                // --- SPANISCH  ---
                ["sp"] = new Dictionary<string, string>
{
                { "puerta", "door_creak.mp3" },
                { "crujió", "door_creak.mp3" },
                { "bosque", "forest_ambience.mp3" },
                { "tree", "forest_ambience.mp3" },
                { "step", "footsteps.mp3" },
                { "walked", "footsteps.mp3" },
                { "ran", "footsteps_running.mp3" },
                { "trueno", "thunder.mp3" },
                { "tormenta", "storm_wind.mp3" },
                { "magia", "magic_sparkle.mp3" },
                { "bruja", "witch_cackle.mp3" },
                { "dragón", "dragon_roar.mp3" },
                { "mar", "ocean_waves.mp3" },
                { "océano", "ocean_waves.mp3" },
                { "agua", "water_splash.mp3" }
            },

                // --- FRANZÖSISCH  ---
                ["fr"] = new Dictionary<string, string>
{
                { "porte", "door_creak.mp3" },
                { "grinçait", "door_creak.mp3" },
                { "forêt", "forest_ambience.mp3" },
                { "arbre", "forest_ambience.mp3" },
                { "pas", "footsteps.mp3" },
                { "marchait", "footsteps.mp3" },
                { "courait", "footsteps_running.mp3" },
                { "tonnerre", "thunder.mp3" },
                { "tempête", "storm_wind.mp3" },
                { "magie", "magic_sparkle.mp3" },
                { "sorcière", "witch_cackle.mp3" },
                { "dragon", "dragon_roar.mp3" },
                { "mer", "ocean_waves.mp3" },
                { "océan", "ocean_waves.mp3" },
                { "eau", "water_splash.mp3" }
            },

                // --- ITALENISCH  ---
                ["it"] = new Dictionary<string, string>
{
                { "porta", "door_creak.mp3" },
                { "scricchiolò", "door_creak.mp3" },
                { "foresta", "forest_ambience.mp3" },
                { "albero", "forest_ambience.mp3" },
                { "passo", "footsteps.mp3" },
                { "camminò", "footsteps.mp3" },
                { "corse", "footsteps_running.mp3" },
                { "tuono", "thunder.mp3" },
                { "tempesta", "storm_wind.mp3" },
                { "magia", "magic_sparkle.mp3" },
                { "strega", "witch_cackle.mp3" },
                { "drago", "dragon_roar.mp3" },
                { "mare", "ocean_waves.mp3" },
                { "oceano", "ocean_waves.mp3" },
                { "acqua", "water_splash.mp3" }
            },

                // --- RUSSISCH  ---
                ["ru"] = new Dictionary<string, string>
{
                { "дверь", "door_creak.mp3" },
                { "скрипела", "door_creak.mp3" },
                { "лес", "forest_ambience.mp3" },
                { "дерево", "forest_ambience.mp3" },
                { "шаг", "footsteps.mp3" },
                { "шел", "footsteps.mp3" },
                { "бежал", "footsteps_running.mp3" },
                { "гром", "thunder.mp3" },
                { "буря", "storm_wind.mp3" },
                { "магия", "magic_sparkle.mp3" },
                { "ведьма", "witch_cackle.mp3" },
                { "дракон", "dragon_roar.mp3" },
                { "море", "ocean_waves.mp3" },
                { "океан", "ocean_waves.mp3" },
                { "вода", "water_splash.mp3" }
            },

                // --- UKRAINISCH  ---
                ["uk"] = new Dictionary<string, string>
{
                { "двері", "door_creak.mp3" },
                { "скрипів", "door_creak.mp3" },
                { "ліс", "forest_ambience.mp3" },
                { "дерево", "forest_ambience.mp3" },
                { "крок", "footsteps.mp3" },
                { "йшов", "footsteps.mp3" },
                { "біг", "footsteps_running.mp3" },
                { "гром", "thunder.mp3" },
                { "буря", "storm_wind.mp3" },
                { "магія", "magic_sparkle.mp3" },
                { "відьма", "witch_cackle.mp3" },
                { "дракон", "dragon_roar.mp3" },
                { "море", "ocean_waves.mp3" },
                { "океан", "ocean_waves.mp3" },
                { "вода", "water_splash.mp3" }
            },


            };
        }

        // Methode zum Umschalten der Sprache (z.B. wenn Nutzer Settings ändert)
        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
        }

        // Der eigentliche Trigger. Spielt Effekt ab, wenn Keyword gefunden wird
        public async Task TriggerSoundForTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // 1. Versuche, das Mapping für die aktuelle Sprache zu holen
            if (!_languageSoundMaps.TryGetValue(CurrentLanguage, out var keywordMap))
            {
                // Fallback auf Englisch, falls Sprache nicht definiert ist
                if (!_languageSoundMaps.TryGetValue("en", out keywordMap)) return;
            }

            var lowerText = text.ToLower();

            foreach (var pair in keywordMap)
            {
                // Wenn das Keyword im Text vorkommt
                if (lowerText.Contains(pair.Key))
                {
                    await PlayEffectAsync(pair.Value);
                    break; // Nur ein Sound pro Textabschnitt
                }
            }
        }

        // Startet Hintergrundmusik (Loop)
        public async Task PlayBackgroundMusicAsync(string fileName, double volume = 0.3)
        {
            if (_musicPlayer != null) _musicPlayer.Stop();

            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);

            _musicPlayer = _audioManager.CreatePlayer(stream);
            _musicPlayer.Loop = true;
            _musicPlayer.Volume = volume;
            _musicPlayer.Play();
        }

        public void StopBackgroundMusic()
        {
            if (_musicPlayer?.IsPlaying == true) _musicPlayer.Stop();
        }

        private async Task PlayEffectAsync(string fileName, double volume = 1.0)
        {
            try
            {
                // Datei öffnen
                using var stream = await FileSystem.OpenAppPackageFileAsync($"Sounds/{fileName}");
                var player = _audioManager.CreatePlayer(stream);

                player.Volume = volume;
                player.Play();

                // NEU: Nutzung des Dispatchers
                // Wir berechnen die Wartezeit (Dauer des Sounds + 1 Sekunde Puffer)
                var delay = TimeSpan.FromSeconds(player.Duration + 1);

                if (Application.Current == null)
                    return;

                Application.Current.Dispatcher.StartTimer(delay, () =>
                {
                    // Diese Aktion wird nach Ablauf der Zeit auf dem UI-Thread ausgeführt
                    player.Dispose();

                    // WICHTIG: 'false' zurückgeben, damit der Timer NICHT erneut läuft (kein Loop)
                    return false;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sound error: {ex.Message}");
            }
        }

    }
}
