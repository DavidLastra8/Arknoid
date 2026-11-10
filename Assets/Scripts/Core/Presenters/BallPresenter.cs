using System;
using Arknoid.Core.Config;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>
    /// Reglas de UNA bola: lanzarla, acelerarla en cada rebote y avisar cuando cae.
    /// Qué pasa cuando cae (quitarla o perder una vida) lo decide BallsPresenter.
    /// </summary>
    public class BallPresenter : IDisposable
    {
        public event Action<BallPresenter> Lost;

        private readonly GameRules rules;
        private readonly IAudioService audio;

        public BallPresenter(IBallView view, GameRules rules, IAudioService audio)
        {
            View = view;
            this.rules = rules;
            this.audio = audio;

            view.Bounced += OnBounced;
            view.HitPaddle += OnHitPaddle;
            view.HitDeathZone += OnHitDeathZone;
        }

        public IBallView View { get; }

        /// <summary>Lanzada y todavía no ha caído.</summary>
        public bool IsInPlay { get; private set; }

        public void Launch(IRandom random)
        {
            View.Launch(random.Range(-1f, 1f), 1f, rules.launchForce);
            IsInPlay = true;
        }

        /// <summary>Vuelve a la salida, parada, a la espera de un nuevo saque.</summary>
        public void Park()
        {
            IsInPlay = false;
            View.ResetToStart();
        }

        private void OnBounced()
        {
            if (View.Speed <= rules.maxSpeedForSpeedUp)
            {
                View.MultiplyVelocity(rules.speedUpFactor);
            }
        }

        private void OnHitPaddle() => audio.Play(GameSound.Hit);

        private void OnHitDeathZone()
        {
            // Una bola puede tocar la zona dos veces antes de desaparecer: solo cuenta la primera.
            if (!IsInPlay) return;

            IsInPlay = false;
            Lost?.Invoke(this);
        }

        public void Dispose()
        {
            View.Bounced -= OnBounced;
            View.HitPaddle -= OnHitPaddle;
            View.HitDeathZone -= OnHitDeathZone;
        }
    }
}
