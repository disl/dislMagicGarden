using static dislMagicGarden.Platforms.Android.AndroidTtsService;

namespace dislMagicGarden.Models
{
    public interface ITextToSpeechService
    {
        Task InitializeAsync();

        Task Speak(string text, TtsLanguage? language = null, TtsGender? gender = null, float rate = 1.0f);
        void Pause();
        void Resume();
        void Stop();
        bool IsSpeaking { get; }
        bool IsPaused { get; }

        void SetLanguage(TtsLanguage language);
        void SetGender(TtsGender gender);
        TtsLanguage GetCurrentLanguage();
        TtsGender GetCurrentGender();
        bool IsInitialized { get; }
    }

    public enum TtsLanguage
    {
        Auto,
        German,
        English,
        Spanish,
        French,
        Russian,
        Ukrainian
    }

    public enum TtsGender
    {
        Female,
        Male,
        Default
    }


}
