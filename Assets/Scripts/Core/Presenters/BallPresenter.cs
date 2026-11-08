using System;
using Arknoid.Core.Config;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Reglas de la pelota: saque con retardo, aceleración en cada rebote y aviso al caer.</summary>
    public class BallPresenter : IBallController, ITickable, IDisposable
    {
        public event Action BallLost;

        private readonly IBallView view;
        private readonly GameRules rules;
        private readonly IRandom random;
        private readonly IAudioService audio;

        private float launchCountdown = -1f;

        public BallPresenter(IBallView view, GameRules rules, IRandom random, IAudioService audio)
        {
            this.view = view;
            this.rules = rules;
            this.random = random;
            this.audio = audio;

            view.Bounced += OnBounced;
            view.HitPaddle += OnHitPaddle;
            view.HitDeathZone += OnHitDeathZone;
        }

        public bool IsWaitingToLaunch => launchCountdown >= 0f;

        public void Serve()
        {
            view.ResetToStart();
            launchCountdown = rules.relaunchDelay;
        }

        public void Stop()
        {
            launchCountdown = -1f;
            view.ResetToStart();
        }

        public void Tick(float deltaTime)
        {
            if (!IsWaitingToLaunch) return;

            launchCountdown -= deltaTime;
            if (launchCountdown <= 0f)
            {
                launchCountdown = -1f;
                view.Launch(random.Range(-1f, 1f), 1f, rules.launchForce);
            }
        }

        private void OnBounced()
        {
            if (view.Speed <= rules.maxSpeedForSpeedUp)
            {
                view.MultiplyVelocity(rules.speedUpFactor);
            }
        }

        private void OnHitPaddle() => audio.Play(GameSound.Hit);

        private void OnHitDeathZone() => BallLost?.Invoke();

        public void Dispose()
        {
            view.Bounced -= OnBounced;
            view.HitPaddle -= OnHitPaddle;
            view.HitDeathZone -= OnHitDeathZone;
        }
    }
}
