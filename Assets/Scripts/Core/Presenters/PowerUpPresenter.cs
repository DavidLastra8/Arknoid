using System;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    public class PowerUpPresenter : IDisposable
    {
        private readonly IPowerUpView view;
        private readonly IPowerUpEffect effect;
        private readonly IAudioService audio;

        public PowerUpPresenter(IPowerUpView view, IPowerUpEffect effect, IAudioService audio)
        {
            this.view = view;
            this.effect = effect;
            this.audio = audio;

            view.Collected += OnCollected;
            view.Missed += OnMissed;
        }

        private void OnCollected()
        {
            audio.Play(GameSound.PowerUp);
            effect.Apply();
            Finish();
        }

        private void OnMissed() => Finish();

        private void Finish()
        {
            Dispose();
            view.Remove();
        }

        public void Dispose()
        {
            view.Collected -= OnCollected;
            view.Missed -= OnMissed;
        }
    }
}
