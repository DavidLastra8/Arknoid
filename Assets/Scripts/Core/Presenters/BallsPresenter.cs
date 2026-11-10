using System;
using System.Collections.Generic;
using Arknoid.Core.Config;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>
    /// Gestiona todas las bolas de la escena: la principal (la que ya estaba en la escena) y las extra
    /// de los power-ups. Solo avisa de BallLost (y por tanto solo se pierde una vida) cuando cae la
    /// ÚLTIMA bola en juego. Mientras quede alguna, las que caen simplemente desaparecen.
    /// </summary>
    public class BallsPresenter : IBallController, IBallSpawner, ITrackable, ITickable, IDisposable
    {
        public event Action BallLost;

        private readonly BallPresenter main;
        private readonly IBallFactory factory;
        private readonly GameRules rules;
        private readonly IRandom random;
        private readonly IAudioService audio;

        private readonly List<BallPresenter> inPlay = new List<BallPresenter>();
        private readonly List<BallPresenter> extras = new List<BallPresenter>();
        private float launchCountdown = -1f;

        // false antes del primer saque y después de Stop() (game over / nivel superado):
        // un multibola recogido en ese mismo frame no debe crear bolas.
        private bool roundActive;

        public BallsPresenter(IBallView mainBall, IBallFactory factory, GameRules rules, IRandom random, IAudioService audio)
        {
            this.factory = factory;
            this.rules = rules;
            this.random = random;
            this.audio = audio;

            main = new BallPresenter(mainBall, rules, audio);
            main.Lost += OnBallLost;
        }

        /// <summary>Bolas en juego, incluida la principal mientras espera el saque.</summary>
        public int BallsInPlay => inPlay.Count;

        public bool IsWaitingToLaunch => launchCountdown >= 0f;

        /// <summary>El modo automático de la pala sigue a la bola más baja (la más cerca de caer).</summary>
        public float PositionX
        {
            get
            {
                BallPresenter lowest = null;
                foreach (var ball in inPlay)
                {
                    if (lowest == null || ball.View.Position.Y < lowest.View.Position.Y) lowest = ball;
                }

                return (lowest ?? main).View.PositionX;
            }
        }

        public void Serve()
        {
            RemoveExtras();
            main.View.SetActive(true);
            main.Park();

            inPlay.Clear();
            inPlay.Add(main);
            launchCountdown = rules.relaunchDelay;
            roundActive = true;
        }

        public void Stop()
        {
            roundActive = false;
            launchCountdown = -1f;
            RemoveExtras();
            inPlay.Clear();
            main.Park();
        }

        public void Tick(float deltaTime)
        {
            if (!IsWaitingToLaunch) return;

            launchCountdown -= deltaTime;
            if (launchCountdown <= 0f)
            {
                launchCountdown = -1f;
                main.Launch(random);
            }
        }

        public void SpawnBalls(int count, BallSpawnOrigin origin)
        {
            if (!roundActive) return;

            int allowed = Math.Min(count, rules.maxBallsInPlay - inPlay.Count);

            for (int i = 0; i < allowed; i++)
            {
                var position = origin == BallSpawnOrigin.BallStart
                    ? main.View.StartPosition
                    : PickSourceBall().View.Position;

                var view = factory.Create(position);
                if (view == null) return;

                var ball = new BallPresenter(view, rules, audio);
                ball.Lost += OnBallLost;
                extras.Add(ball);
                inPlay.Add(ball);
                ball.Launch(random);
            }
        }

        private BallPresenter PickSourceBall()
        {
            var moving = inPlay.FindAll(b => b.IsInPlay);
            if (moving.Count == 0) return main;

            int index = Math.Min((int)(random.Value01() * moving.Count), moving.Count - 1);
            return moving[index];
        }

        private void OnBallLost(BallPresenter ball)
        {
            if (!inPlay.Remove(ball)) return;

            if (ball == main)
            {
                // La principal no se destruye: se esconde hasta el próximo saque.
                if (inPlay.Count > 0) main.View.SetActive(false);
            }
            else
            {
                DestroyExtra(ball);
            }

            if (inPlay.Count == 0) BallLost?.Invoke();
        }

        private void RemoveExtras()
        {
            foreach (var ball in extras.ToArray()) DestroyExtra(ball);
        }

        private void DestroyExtra(BallPresenter ball)
        {
            ball.Lost -= OnBallLost;
            ball.Dispose();
            extras.Remove(ball);
            inPlay.Remove(ball);
            ball.View.Remove();
        }

        public void Dispose()
        {
            main.Lost -= OnBallLost;
            main.Dispose();
            foreach (var ball in extras)
            {
                ball.Lost -= OnBallLost;
                ball.Dispose();
            }

            extras.Clear();
            inPlay.Clear();
        }
    }
}
