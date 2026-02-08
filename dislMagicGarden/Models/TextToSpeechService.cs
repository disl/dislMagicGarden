namespace dislMagicGarden.Models
{
    public interface ITextToSpeechService
    {
        Task Speak(string text);
        void Pause();
        void Resume();
        void Stop();
        bool IsSpeaking { get; }
        bool IsPaused { get; }
    }


}
