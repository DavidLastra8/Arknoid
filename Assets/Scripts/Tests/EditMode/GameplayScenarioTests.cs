using System.Collections.Generic;
using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Presenters;
using Arknoid.Core.Services;
using NUnit.Framework;

namespace Arknoid.Core.Tests
{
    /// <summary>
    /// Escenarios de partida completos: los presenters y modelos REALES conectados entre sí como en
    /// SceneInstaller (bolas, flujo de partida, barrera, efectos, autoguardado). Solo las vistas y los
    /// servicios son fakes. Sirven para detectar fallos de combinación: varios power-ups seguidos,
    /// power-ups al acabar la partida, bolas que caen a la vez, etc.
    /// </summary>
    public class GameplayScenarioTests
    {
        private TestGame game;

        [SetUp]
        public void SetUp()
        {
            game = new TestGame(lives: 3, bricks: 5);
            game.StartAndLaunch();
        }

        // ---------- Varios power-ups seguidos ----------

        [Test]
        public void AllFivePowerUpsInTheSameFrame_AllApplyCorrectly()
        {
            game.Collect(PowerUpType.ExtraLife);
            game.Collect(PowerUpType.MultiBallFromStart);
            game.Collect(PowerUpType.MultiBallFromBall);
            game.Collect(PowerUpType.Barrier);
            game.Collect(PowerUpType.BonusScore);

            Assert.AreEqual(4, game.Lives.Current);
            Assert.AreEqual(1 + game.Settings.ballsFromStart + game.Settings.ballsFromBall, game.Balls.BallsInPlay);
            Assert.IsTrue(game.Barrier.IsActive);
            Assert.AreEqual(game.Settings.bonusScore, game.Score.Current);
            Assert.IsFalse(game.Flow.IsFinished);
        }

        [Test]
        public void ManyMultiBallsInARow_NeverExceedTheLimit()
        {
            for (int i = 0; i < 20; i++)
            {
                game.Collect(i % 2 == 0 ? PowerUpType.MultiBallFromStart : PowerUpType.MultiBallFromBall);
            }

            Assert.AreEqual(game.Rules.maxBallsInPlay, game.Balls.BallsInPlay);
            Assert.AreEqual(game.Rules.maxBallsInPlay - 1, game.BallFactory.Created.Count);
        }

        [Test]
        public void ManyBarriersInARow_RenewDurationWithoutStacking()
        {
            game.Collect(PowerUpType.Barrier);
            game.Tick(game.Settings.barrierDuration - 1f);
            game.Collect(PowerUpType.Barrier);
            game.Collect(PowerUpType.Barrier);

            Assert.AreEqual(game.Settings.barrierDuration, game.Barrier.RemainingTime, 0.001f);

            game.Tick(game.Settings.barrierDuration + 0.1f);
            Assert.IsFalse(game.Barrier.IsActive, "Se desactiva aunque se haya cogido 3 veces");
            Assert.IsFalse(game.BarrierView.Visible);
        }

        [Test]
        public void BonusScoreInARow_AddsEveryTimeAndUpdatesBest()
        {
            for (int i = 0; i < 5; i++) game.Collect(PowerUpType.BonusScore);

            Assert.AreEqual(5 * game.Settings.bonusScore, game.Score.Current);
            Assert.AreEqual(game.Score.Current, game.Score.Best);
        }

        [Test]
        public void SamePowerUpCollectedAndMissedInTheSameFrame_AppliesOnlyOnce()
        {
            var powerUp = game.Collect(PowerUpType.ExtraLife);
            powerUp.Collect();
            powerUp.Miss();

            Assert.AreEqual(4, game.Lives.Current);
            Assert.IsTrue(powerUp.Removed);
        }

        // ---------- Vidas con varias bolas ----------

        [Test]
        public void ExtraBallsFalling_DoNotCostLives()
        {
            game.Collect(PowerUpType.MultiBallFromStart);

            foreach (var extra in game.BallFactory.Created) extra.RaiseDeath();

            Assert.AreEqual(3, game.Lives.Current);
            Assert.AreEqual(1, game.Balls.BallsInPlay);
            Assert.IsTrue(game.BallFactory.Created.TrueForAll(b => b.Removed));
        }

        [Test]
        public void MainBallFirstThenExtras_ExactlyOneLifeLost_AndNewServeIsClean()
        {
            game.Collect(PowerUpType.MultiBallFromStart);

            game.MainBall.RaiseDeath();
            Assert.AreEqual(3, game.Lives.Current, "Quedan bolas extra");
            Assert.IsFalse(game.MainBall.Active);

            foreach (var extra in game.BallFactory.Created) extra.RaiseDeath();

            Assert.AreEqual(2, game.Lives.Current, "Solo una vida al caer la última");
            Assert.IsTrue(game.MainBall.Active, "El nuevo saque reactiva la principal");
            Assert.AreEqual(1, game.Balls.BallsInPlay);
            Assert.IsTrue(game.Balls.IsWaitingToLaunch);
        }

        [Test]
        public void TwoBallsFallingInTheSameFrame_OnlyOneLifeLost()
        {
            game.Collect(PowerUpType.MultiBallFromBall);
            var all = new List<FakeBallView> { game.MainBall };
            all.AddRange(game.BallFactory.Created);

            foreach (var ball in all) ball.RaiseDeath();

            Assert.AreEqual(2, game.Lives.Current);
        }

        [Test]
        public void SameBallTouchingDeathColliderTwice_CountsOnce()
        {
            game.Collect(PowerUpType.MultiBallFromStart);
            var extra = game.BallFactory.Created[0];

            extra.RaiseDeath();
            extra.RaiseDeath(); // segundo contacto antes de destruirse
            game.MainBall.RaiseDeath();
            game.MainBall.RaiseDeath();

            Assert.AreEqual(3, game.Lives.Current, "Aún queda otra bola extra");
            Assert.AreEqual(1, game.Balls.BallsInPlay);
        }

        [Test]
        public void LosingAllLives_WithExtras_GoesToGameOverAndRemovesThem()
        {
            var g = new TestGame(lives: 1, bricks: 5);
            g.StartAndLaunch();
            g.Collect(PowerUpType.MultiBallFromStart);
            g.MainBall.RaiseDeath();
            foreach (var extra in g.BallFactory.Created) extra.RaiseDeath();

            Assert.AreEqual(SceneNames.GameOver, g.Scenes.LastLoaded);
            Assert.IsTrue(g.Runs.Cleared);
            Assert.IsTrue(g.Flow.IsFinished);
        }

        // ---------- Power-ups en el límite de la partida ----------

        [Test]
        public void MultiBallCollectedRightAfterLevelCleared_DoesNotSpawnBalls()
        {
            game.ClearLevel();
            game.Collect(PowerUpType.MultiBallFromStart);
            game.Collect(PowerUpType.MultiBallFromBall);

            Assert.AreEqual(0, game.BallFactory.Created.Count);
        }

        [Test]
        public void ExtraLifeCollectedRightAfterGameOver_DoesNotResurrectTheSavedRun()
        {
            var g = new TestGame(lives: 1, bricks: 5);
            g.StartAndLaunch();
            g.MainBall.RaiseDeath();
            Assert.IsFalse(g.Runs.HasRun, "Game over borra la partida");

            g.Collect(PowerUpType.ExtraLife); // en el mismo frame, antes de cargar GameOver

            Assert.IsFalse(g.Runs.HasRun, "No debe volver a guardarse");
        }

        [Test]
        public void ClearingLevelAfterPowerUps_SavesNextLevelWithBonusScore()
        {
            game.Collect(PowerUpType.BonusScore);
            game.Collect(PowerUpType.ExtraLife);
            game.ClearLevel();

            Assert.AreEqual(SceneNames.Level2, game.Runs.Stored.SceneName);
            Assert.AreEqual(game.Score.Current, game.Runs.Stored.Score);
            Assert.AreEqual(4, game.Runs.Stored.Lives);
            Assert.IsTrue(game.Runs.Stored.Level == null, "El nivel siguiente empieza completo");
        }

        [Test]
        public void MultiBallDuringServeCountdown_MainBallStillLaunches()
        {
            game.MainBall.RaiseDeath(); // pierde vida → nuevo saque con cuenta atrás
            Assert.IsTrue(game.Balls.IsWaitingToLaunch);
            int launchesBefore = game.MainBall.LaunchCount;

            game.Collect(PowerUpType.MultiBallFromBall); // no hay bola moviéndose: sale desde la salida
            Assert.AreEqual(game.MainBall.StartPosition.Y, game.BallFactory.Positions[0].Y);

            game.Tick(game.Rules.relaunchDelay + 0.1f);
            Assert.AreEqual(launchesBefore + 1, game.MainBall.LaunchCount);
        }

        // ---------- Valores raros en el catálogo ----------

        [Test]
        public void ZeroOrNegativeSettings_DoNothingAndDoNotBreak()
        {
            game.Settings.extraLives = -2;
            game.Settings.ballsFromStart = 0;
            game.Settings.ballsFromBall = -5;
            game.Settings.barrierDuration = -1f;
            game.Settings.bonusScore = -100;

            foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType))) game.Collect(type);

            Assert.AreEqual(3, game.Lives.Current);
            Assert.AreEqual(1, game.Balls.BallsInPlay);
            Assert.IsFalse(game.Barrier.IsActive);
            Assert.AreEqual(0, game.Score.Current);
        }

        [Test]
        public void DropsAcrossAWholeLevel_RespectMaximumAndUseEveryType()
        {
            var random = new SequenceRandom();
            var factory = new FakePowerUpFactory();
            foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType))) factory.Types.Add(type);

            var counts = new Dictionary<PowerUpType, int>();
            for (int levelRun = 0; levelRun < 200; levelRun++)
            {
                var policy = new PowerUpDropPolicy(0.24f, 2, random);
                var spawner = new PowerUpSpawner(policy, new RandomPowerUpSelector(factory.Types, random), factory);
                int before = factory.Created.Count;

                for (int brick = 0; brick < 30; brick++) spawner.TrySpawnAt(new WorldPoint(0f, 0f, 0f));

                Assert.IsTrue(factory.Created.Count - before <= 2, "Máximo 2 por nivel");
            }

            foreach (var t in factory.Created) counts[t] = counts.TryGetValue(t, out int c) ? c + 1 : 1;
            foreach (var type in factory.Types) Assert.IsTrue(counts.ContainsKey(type), "Nunca salió " + type);
        }

        /// <summary>Pseudoaleatorio determinista (LCG) para que el test dé siempre lo mismo.</summary>
        private class SequenceRandom : IRandom
        {
            private uint state = 12345;

            public float Value01()
            {
                state = state * 1664525u + 1013904223u;
                return (state >> 8) / 16777216f;
            }

            public float Range(float min, float max) => min + (max - min) * Value01();
        }

        /// <summary>Monta una partida como SceneInstaller, pero con vistas fake.</summary>
        private class TestGame
        {
            public readonly GameRules Rules = new GameRules();
            public readonly PowerUpSettings Settings = new PowerUpSettings();
            public readonly FakeAudio Audio = new FakeAudio();
            public readonly FakeRandom Random = new FakeRandom { NextValue01 = 0.5f };
            public readonly FakeBallView MainBall = new FakeBallView { PositionY = 1f };
            public readonly FakeBallFactory BallFactory = new FakeBallFactory();
            public readonly FakeBarrierView BarrierView = new FakeBarrierView();
            public readonly FakeSceneLoader Scenes = new FakeSceneLoader();
            public readonly FakeRunRepository Runs = new FakeRunRepository { HasRun = true };
            public readonly ScoreModel Score = new ScoreModel(0, 0);
            public readonly LivesModel Lives;
            public readonly LevelModel Level = new LevelModel();
            public readonly BallsPresenter Balls;
            public readonly BarrierPresenter Barrier;
            public readonly GameFlowPresenter Flow;
            public readonly PowerUpEffects Effects;

            public TestGame(int lives, int bricks)
            {
                Lives = new LivesModel(lives);
                for (int i = 0; i < bricks; i++) Level.Register("b" + i, new BrickModel(1, 10));

                Balls = new BallsPresenter(MainBall, BallFactory, Rules, Random, Audio);
                Barrier = new BarrierPresenter(BarrierView);
                Flow = new GameFlowPresenter(Lives, Score, Level, Balls, new FakePaddleController(), Audio,
                    Scenes, Runs, LevelSequence.Default(), SceneNames.Level1);

                Effects = new PowerUpEffects()
                    .Register(PowerUpType.ExtraLife, new ExtraLifeEffect(Lives, Settings))
                    .Register(PowerUpType.MultiBallFromStart,
                        new SpawnBallsEffect(Balls, BallSpawnOrigin.BallStart, () => Settings.ballsFromStart))
                    .Register(PowerUpType.MultiBallFromBall,
                        new SpawnBallsEffect(Balls, BallSpawnOrigin.ExistingBall, () => Settings.ballsFromBall))
                    .Register(PowerUpType.Barrier, new BarrierEffect(Barrier, Settings))
                    .Register(PowerUpType.BonusScore, new BonusScoreEffect(Score, Settings));

                new RunAutoSaver(Runs, SceneNames.Level1, 1u, Score, Lives, Level,
                    new PowerUpDropPolicy(0f, 0, Random), null, Flow);
            }

            public void StartAndLaunch()
            {
                Flow.StartRound();
                Tick(Rules.relaunchDelay + 0.01f);
            }

            public void Tick(float dt)
            {
                Balls.Tick(dt);
                Barrier.Tick(dt);
            }

            public FakePowerUpView Collect(PowerUpType type)
            {
                var view = new FakePowerUpView { Type = type };
                new PowerUpPresenter(view, Effects.For(type), Audio);
                view.Collect();
                return view;
            }

            public void ClearLevel()
            {
                while (!Level.IsCleared) Level.NotifyBrickDestroyed();
            }
        }
    }
}
