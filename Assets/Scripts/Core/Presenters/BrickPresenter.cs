using System;
using Arknoid.Core.Models;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Un presenter por ladrillo: aplica daño, suma puntos, suelta power-ups y avisa al nivel.</summary>
    public class BrickPresenter : IDisposable
    {
        private readonly IBrickView view;
        private readonly BrickModel brick;
        private readonly ScoreModel score;
        private readonly LevelModel level;
        private readonly PowerUpDropPolicy dropPolicy;
        private readonly IAudioService audio;
        private readonly int damage;
        private readonly Action<IPowerUpView> onPowerUpSpawned;

        public BrickPresenter(
            IBrickView view,
            BrickModel brick,
            ScoreModel score,
            LevelModel level,
            PowerUpDropPolicy dropPolicy,
            IAudioService audio,
            int damage,
            Action<IPowerUpView> onPowerUpSpawned)
        {
            this.view = view;
            this.brick = brick;
            this.score = score;
            this.level = level;
            this.dropPolicy = dropPolicy;
            this.audio = audio;
            this.damage = damage;
            this.onPowerUpSpawned = onPowerUpSpawned;

            level.Register(view.Id, brick);
            view.HitByBall += OnHitByBall;
        }

        private void OnHitByBall()
        {
            if (brick.IsDestroyed) return;

            audio.Play(GameSound.Hit);
            score.Add(brick.ScoreValue);

            if (!brick.TakeHit(damage)) return;

            if (dropPolicy.TryDrop())
            {
                var powerUp = view.SpawnPowerUp();
                if (powerUp != null) onPowerUpSpawned?.Invoke(powerUp);
            }

            Dispose();
            view.Break();
            level.NotifyBrickDestroyed();
        }

        public void Dispose()
        {
            view.HitByBall -= OnHitByBall;
        }
    }
}
