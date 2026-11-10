using System;
using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Services;

namespace Arknoid.Core.Presenters
{
    /// <summary>
    /// Flujo de una partida en un nivel: perder vidas, game over y paso al siguiente nivel.
    /// Es la lógica que antes estaba repartida entre BallManager, BrickManager y GameManager.
    /// </summary>
    public class GameFlowPresenter : IDisposable
    {
        private readonly LivesModel lives;
        private readonly ScoreModel score;
        private readonly LevelModel level;
        private readonly IBallController ball;
        private readonly IPaddleController paddle;
        private readonly IAudioService audio;
        private readonly ISceneLoader scenes;
        private readonly IRunRepository runs;
        private readonly LevelSequence levels;
        private readonly string currentLevel;

        public GameFlowPresenter(
            LivesModel lives,
            ScoreModel score,
            LevelModel level,
            IBallController ball,
            IPaddleController paddle,
            IAudioService audio,
            ISceneLoader scenes,
            IRunRepository runs,
            LevelSequence levels,
            string currentLevel)
        {
            this.lives = lives;
            this.score = score;
            this.level = level;
            this.ball = ball;
            this.paddle = paddle;
            this.audio = audio;
            this.scenes = scenes;
            this.runs = runs;
            this.levels = levels;
            this.currentLevel = currentLevel;

            ball.BallLost += OnBallLost;
            level.Cleared += OnLevelCleared;
        }

        public bool IsFinished { get; private set; }

        public void StartRound()
        {
            paddle.ResetPosition();
            ball.Serve();
        }

        private void OnBallLost()
        {
            if (IsFinished) return;

            audio.Play(GameSound.LifeLost);
            lives.LoseOne();

            if (lives.IsDepleted)
            {
                Finish();
                runs.Clear();
                scenes.Load(SceneNames.GameOver);
            }
            else
            {
                StartRound();
            }
        }

        private void OnLevelCleared()
        {
            if (IsFinished) return;
            Finish();

            if (levels.TryGetStep(currentLevel, out var step))
            {
                runs.Save(new RunData(step.NextLevel, score.Current, lives.Current));
                scenes.Load(step.TransitionScene);
            }
            else
            {
                scenes.Load(SceneNames.MainMenu);
            }
        }

        private void Finish()
        {
            IsFinished = true;
            ball.Stop();
        }

        public void Dispose()
        {
            ball.BallLost -= OnBallLost;
            level.Cleared -= OnLevelCleared;
        }
    }
}
