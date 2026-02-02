namespace dislMagicGarden.Services
{
    public interface ITextToSpeechService
    {
        // rate: Die gewünschte Geschwindigkeit (z.B. 0.5f für halb so schnell)
        Task SpeakAsync(string text);
        void Pause();
        void Resume();
        void Stop();
        void SetRate(float rate);
    }
}
