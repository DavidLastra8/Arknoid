using System;
using Arknoid.Core.Models;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Un presenter por ladrillo: aplica daño, suma puntos, intenta soltar power-up y avisa al nivel.</summary>
    public class BrickPresenter : IDisposable
    {
        private readonly IBrickView view;
        private readonly BrickModel brick;
        private readonly ScoreModel score;
        private readonly LevelModel level;
        private readonly PowerUpSpawner powerUps;
        private readonly IAudioService audio;
        private readonly int damage;

        public BrickPresenter(
            IBrickView view,
            BrickModel brick,
            ScoreModel score,
            LevelModel level,
            PowerUpSpawner powerUps,
            IAudioService audio,
            int damage)
        {
            this.view = view;
            this.brick = brick;
            this.score = score;
            this.level = level;
            this.powerUps = powerUps;
            this.audio = audio;
            this.damage = damage;

            level.Register(view.Id, brick);
            view.HitByBall += OnHitByBall;
        }

        private void OnHitByBall()
        {
            if (brick.IsDestroyed) return;

            audio.Play(GameSound.Hit);
            score.Add(brick.ScoreValue);

            if (!brick.TakeHit(damage)) return;

            powerUps.TrySpawnAt(view.Position);

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
