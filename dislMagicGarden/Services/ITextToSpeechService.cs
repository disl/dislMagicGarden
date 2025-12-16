namespace dislMagicGarden.Services
{
    public interface ITextToSpeechService
    {
        // rate: Die gewünschte Geschwindigkeit (z.B. 0.5f für halb so schnell)
        Task Speak(string text, float speed);

        void Stop();
    }
}
