namespace Arknoid.Core.Services
{
    public enum GameSound
    {
        Hit,
        LifeLost,
        PowerUp,
    }

    public interface IAudioService
    {
        void Play(GameSound sound);
    }

    /// <summary>Null Object: se usa si la escena no tiene AudioService (p. ej. al dar Play directamente en Lvl1).</summary>
    public sealed class NullAudioService : IAudioService
    {
        public void Play(GameSound sound) { }
    }
}
