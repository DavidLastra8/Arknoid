using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Presenters;
using Arknoid.Core.Services;
using NUnit.Framework;

namespace Arknoid.Core.Tests
{
    public class PresenterTests
    {
        private GameRules rules;
        private FakeAudio audio;

        [SetUp]
        public void SetUp()
        {
            rules = new GameRules();
            audio = new FakeAudio();
        }

        [Test]
        public void Brick_EachHitAddsItsScoreOnce()
        {
            var view = new FakeBrickView { HitPoints = 3, ScoreValue = 10 };
            var score = new ScoreModel(0, 0);
            var level = new LevelModel();
            var drops = new PowerUpDropPolicy(0f, 0, new FakeRandom());
            new BrickPresenter(view, new BrickModel(view.HitPoints, view.ScoreValue), score, level, drops, audio, 1, null);

            view.Hit();
            view.Hit();

            // Antes se sumaba la puntuación acumulada del ladrillo (10, luego 20 → 30).
            Assert.AreEqual(20, score.Current);
            Assert.IsFalse(view.Broken);
        }

        [Test]
        public void Brick_BreaksNotifiesLevelAndSpawnsPowerUp()
        {
            var view = new FakeBrickView { HitPoints = 1 };
            var level = new LevelModel();
            bool cleared = false;
            level.Cleared += () => cleared = true;
            var drops = new PowerUpDropPolicy(1f, 1, new FakeRandom { NextValue01 = 0f });
            object spawned = null;

            new BrickPresenter(view, new BrickModel(1, 10), new ScoreModel(0, 0), level, drops, audio, 1, p => spawned = p);
            view.Hit();
            view.Hit(); // ya está roto: no debe contar dos veces

            Assert.IsTrue(view.Broken);
            Assert.IsTrue(cleared);
            Assert.AreEqual(1, view.SpawnCount);
            Assert.AreSame(view.NextPowerUp, spawned);
        }

        [Test]
        public void PowerUp_CollectedAppliesEffectAndRemovesView()
        {
            var view = new FakePowerUpView();
            var lives = new LivesModel(1);
            new PowerUpPresenter(view, new ExtraLifeEffect(lives, 1), audio);

            view.Collect();

            Assert.AreEqual(2, lives.Current);
            Assert.IsTrue(view.Removed);
            Assert.AreEqual(GameSound.PowerUp, audio.Played[0]);
        }

        [Test]
        public void Ball_LaunchesAfterDelayAndSpeedsUpUntilLimit()
        {
            var view = new FakeBallView();
            var presenter = new BallPresenter(view, rules, new FakeRandom(), audio);

            presenter.Serve();
            presenter.Tick(rules.relaunchDelay * 0.5f);
            Assert.AreEqual(0, view.LaunchCount);
            presenter.Tick(rules.relaunchDelay);
            Assert.AreEqual(1, view.LaunchCount);
            Assert.AreEqual(rules.launchForce, view.LastForce);

            view.Speed = rules.maxSpeedForSpeedUp + 1f;
            view.RaiseBounce();
            Assert.AreEqual(1f, view.VelocityFactor);

            view.Speed = 1f;
            view.RaiseBounce();
            Assert.AreEqual(rules.speedUpFactor, view.VelocityFactor, 0.0001f);
        }

        [Test]
        public void Paddle_SliderMapsToRangeAndAutoplayFollowsBall()
        {
            var view = new FakePaddleView();
            var ball = new FakeBallView { PositionX = 2.5f };
            var input = new FakeInput();
            var presenter = new PaddlePresenter(view, ball, input, rules);

            view.MoveSlider(1f);
            Assert.AreEqual(rules.paddleMaxX, view.X, 0.0001f);

            input.AutoplayTogglePressed = true;
            presenter.Tick(0.016f);
            input.AutoplayTogglePressed = false;
            presenter.Tick(0.016f);

            Assert.IsTrue(presenter.IsAutomatic);
            Assert.AreEqual(2.5f, view.X, 0.0001f);
        }

        [Test]
        public void GameFlow_LosingLastLifeGoesToGameOverAndClearsRun()
        {
            var lives = new LivesModel(1);
            var ball = new FakeBallController();
            var scenes = new FakeSceneLoader();
            var runs = new FakeRunRepository { HasRun = true };
            var flow = new GameFlowPresenter(lives, new ScoreModel(0, 0), new LevelModel(), ball,
                new FakePaddleController(), audio, scenes, runs, LevelSequence.Default(), SceneNames.Level1);

            ball.Lose();

            Assert.IsTrue(flow.IsFinished);
            Assert.IsTrue(ball.Stopped);
            Assert.IsTrue(runs.Cleared);
            Assert.AreEqual(SceneNames.GameOver, scenes.LastLoaded);
        }

        [Test]
        public void GameFlow_LosingALifeServesAgain()
        {
            var ball = new FakeBallController();
            var paddle = new FakePaddleController();
            var flow = new GameFlowPresenter(new LivesModel(3), new ScoreModel(0, 0), new LevelModel(), ball,
                paddle, audio, new FakeSceneLoader(), new FakeRunRepository(), LevelSequence.Default(), SceneNames.Level1);

            flow.StartRound();
            ball.Lose();

            Assert.AreEqual(2, ball.ServeCount);
            Assert.AreEqual(2, paddle.ResetCount);
            Assert.AreEqual(GameSound.LifeLost, audio.Played[0]);
        }

        [Test]
        public void GameFlow_ClearingLevelSavesRunAndLoadsTransition()
        {
            var level = new LevelModel();
            level.RegisterBrick();
            var scenes = new FakeSceneLoader();
            var runs = new FakeRunRepository();
            var score = new ScoreModel(0, 0);
            score.Add(120);
            new GameFlowPresenter(new LivesModel(2), score, level, new FakeBallController(),
                new FakePaddleController(), audio, scenes, runs, LevelSequence.Default(), SceneNames.Level1);

            level.NotifyBrickDestroyed();

            Assert.AreEqual(SceneNames.TransitionToLevel2, scenes.LastLoaded);
            Assert.AreEqual(SceneNames.Level2, runs.Stored.SceneName);
            Assert.AreEqual(120, runs.Stored.Score);
            Assert.AreEqual(2, runs.Stored.Lives);
        }

        [Test]
        public void Pause_EscTogglesMenuAndTime()
        {
            var view = new FakePauseViewSpy();
            var input = new FakeInput();
            var time = new FakeTime();
            var presenter = new PausePresenter(new PauseModel(), view, input, time);

            input.PauseTogglePressed = true;
            presenter.Tick(0f);
            Assert.IsTrue(view.Visible);
            Assert.IsTrue(time.Paused);

            view.ClickResume();
            Assert.IsFalse(view.Visible);
            Assert.IsFalse(time.Paused);
        }

        [Test]
        public void Navigation_NewGameSavesFreshRunAndLoadsFirstLevel()
        {
            var view = new FakeNavigationViewSpy();
            var scenes = new FakeSceneLoader();
            var runs = new FakeRunRepository();
            new NavigationPresenter(view, scenes, runs, LevelSequence.Default(), rules);

            view.ClickNewGame();

            Assert.AreEqual(SceneNames.Level1, scenes.LastLoaded);
            Assert.AreEqual(rules.initialLives, runs.Stored.Lives);
            Assert.AreEqual(0, runs.Stored.Score);
        }

        private class FakePauseViewSpy : Views.IPauseView
        {
            public event System.Action ResumeRequested;
            public bool Visible;
            public void SetPauseMenuVisible(bool visible) => Visible = visible;
            public void ClickResume() => ResumeRequested?.Invoke();
        }

        private class FakeNavigationViewSpy : Views.INavigationView
        {
            public event System.Action NewGameRequested;
            public event System.Action ContinueRequested { add { } remove { } }
            public event System.Action NextLevelRequested { add { } remove { } }
            public event System.Action MainMenuRequested { add { } remove { } }
            public void ClickNewGame() => NewGameRequested?.Invoke();
        }
    }
}
