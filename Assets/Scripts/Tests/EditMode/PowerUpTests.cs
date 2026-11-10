using System.Collections.Generic;
using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Presenters;
using Arknoid.Core.Services;
using NUnit.Framework;

namespace Arknoid.Core.Tests
{
    public class PowerUpTests
    {
        private GameRules rules;
        private FakeAudio audio;
        private FakeRandom random;

        [SetUp]
        public void SetUp()
        {
            rules = new GameRules();
            audio = new FakeAudio();
            random = new FakeRandom();
        }

        // --- Multibola: solo se pierde vida cuando no queda ninguna bola ---

        [Test]
        public void Balls_LosingAnExtraBallDoesNotCostALife()
        {
            var main = new FakeBallView();
            var factory = new FakeBallFactory();
            var balls = new BallsPresenter(main, factory, rules, random, audio);
            int lost = 0;
            balls.BallLost += () => lost++;

            ServeAndLaunch(balls);
            balls.SpawnBalls(2, BallSpawnOrigin.BallStart);
            Assert.AreEqual(3, balls.BallsInPlay);

            factory.Created[0].RaiseDeath();
            main.RaiseDeath();
            Assert.AreEqual(0, lost, "Queda una bola extra: no se pierde vida");
            Assert.IsTrue(factory.Created[0].Removed, "Las extra que caen se destruyen");
            Assert.IsFalse(main.Active, "La principal se esconde hasta el siguiente saque");

            factory.Created[1].RaiseDeath();
            Assert.AreEqual(1, lost, "Ha caído la última bola");
        }

        [Test]
        public void Balls_ServeAfterLosingReactivatesMainAndClearsExtras()
        {
            var main = new FakeBallView();
            var factory = new FakeBallFactory();
            var balls = new BallsPresenter(main, factory, rules, random, audio);

            ServeAndLaunch(balls);
            balls.SpawnBalls(1, BallSpawnOrigin.BallStart);
            main.RaiseDeath();
            balls.Serve();

            Assert.IsTrue(main.Active);
            Assert.IsTrue(factory.Created[0].Removed);
            Assert.AreEqual(1, balls.BallsInPlay);
        }

        [Test]
        public void Balls_SpawnOriginStartOrExistingBall()
        {
            var main = new FakeBallView { StartPosition = new WorldPoint(0f, -4f, 0f) };
            var factory = new FakeBallFactory();
            var balls = new BallsPresenter(main, factory, rules, random, audio);
            ServeAndLaunch(balls);
            main.PositionX = 3f; // la bola se ha movido tras el saque
            main.PositionY = 1f;

            balls.SpawnBalls(1, BallSpawnOrigin.BallStart);
            Assert.AreEqual(-4f, factory.Positions[0].Y);

            balls.SpawnBalls(1, BallSpawnOrigin.ExistingBall);
            Assert.AreEqual(3f, factory.Positions[1].X);
            Assert.AreEqual(1f, factory.Positions[1].Y);
            Assert.AreEqual(1, factory.Created[1].LaunchCount, "Las bolas nuevas salen lanzadas");
        }

        [Test]
        public void Balls_RespectMaximumInPlay()
        {
            rules.maxBallsInPlay = 3;
            var factory = new FakeBallFactory();
            var balls = new BallsPresenter(new FakeBallView(), factory, rules, random, audio);
            ServeAndLaunch(balls);

            balls.SpawnBalls(10, BallSpawnOrigin.BallStart);

            Assert.AreEqual(3, balls.BallsInPlay);
            Assert.AreEqual(2, factory.Created.Count);
        }

        [Test]
        public void Balls_PaddleFollowsLowestBall()
        {
            var main = new FakeBallView();
            var factory = new FakeBallFactory();
            var balls = new BallsPresenter(main, factory, rules, random, audio);
            ServeAndLaunch(balls);
            main.PositionX = 0f;
            main.PositionY = 2f;
            balls.SpawnBalls(1, BallSpawnOrigin.BallStart);
            factory.Created[0].PositionX = -2f;
            factory.Created[0].PositionY = -1f;

            Assert.AreEqual(-2f, balls.PositionX);
        }

        // --- Barrera ---

        [Test]
        public void Barrier_BouncesBallsOnlyWhileActiveAndExpires()
        {
            var view = new FakeBarrierView();
            var barrier = new BarrierPresenter(view);
            var ball = new FakeBallView();
            Assert.IsFalse(view.Visible, "Empieza oculta");

            barrier.Activate(5f);
            view.Touch(ball);
            Assert.IsTrue(view.Visible);
            Assert.AreEqual(1, ball.BounceUpCount);

            barrier.Tick(5.1f);
            Assert.IsFalse(view.Visible);
            view.Touch(ball);
            Assert.AreEqual(1, ball.BounceUpCount, "Desactivada ya no rebota");
        }

        // --- Efectos ---

        [Test]
        public void Effects_UseEditableSettings()
        {
            var settings = new PowerUpSettings { ballsFromStart = 4, ballsFromBall = 6, barrierDuration = 3f, bonusScore = 250, extraLives = 2 };
            var spawner = new FakeBallSpawner();
            var barrier = new FakeBarrierController();
            var score = new ScoreModel(100, 0);
            var lives = new LivesModel(1);

            new SpawnBallsEffect(spawner, BallSpawnOrigin.BallStart, () => settings.ballsFromStart).Apply();
            Assert.AreEqual(4, spawner.Count);
            Assert.AreEqual(BallSpawnOrigin.BallStart, spawner.Origin);

            new SpawnBallsEffect(spawner, BallSpawnOrigin.ExistingBall, () => settings.ballsFromBall).Apply();
            Assert.AreEqual(6, spawner.Count);
            Assert.AreEqual(BallSpawnOrigin.ExistingBall, spawner.Origin);

            new BarrierEffect(barrier, settings).Apply();
            Assert.AreEqual(3f, barrier.Duration);

            new BonusScoreEffect(score, settings).Apply();
            Assert.AreEqual(350, score.Current);

            settings.extraLives = 3; // se lee al aplicar: se puede cambiar en Play
            new ExtraLifeEffect(lives, settings).Apply();
            Assert.AreEqual(4, lives.Current);
        }

        // --- Selección aleatoria entre todos ---

        [Test]
        public void Selector_PicksEveryTypeWithSameProbability()
        {
            var types = new List<PowerUpType>
            {
                PowerUpType.ExtraLife, PowerUpType.MultiBallFromStart, PowerUpType.MultiBallFromBall,
                PowerUpType.Barrier, PowerUpType.BonusScore,
            };
            var selector = new RandomPowerUpSelector(types, random);
            var counts = new Dictionary<PowerUpType, int>();

            for (int i = 0; i < 1000; i++)
            {
                random.NextValue01 = i / 1000f;
                var picked = selector.Pick();
                counts[picked] = counts.TryGetValue(picked, out int c) ? c + 1 : 1;
            }

            foreach (var type in types) Assert.AreEqual(200, counts[type], type.ToString());
        }

        [Test]
        public void Spawner_DoesNothingWhenNoTypesAreAvailable()
        {
            var factory = new FakePowerUpFactory();
            var policy = new PowerUpDropPolicy(1f, 5, random);
            var spawner = new PowerUpSpawner(policy, new RandomPowerUpSelector(factory.Types, random), factory);

            spawner.TrySpawnAt(new WorldPoint(0f, 0f, 0f));

            Assert.AreEqual(0, factory.Created.Count);
            Assert.AreEqual(0, policy.DropsSoFar, "No gasta el cupo de power-ups si no hay ninguno");
        }

        [Test]
        public void Effects_UnknownTypeDoesNothing()
        {
            var effects = new PowerUpEffects();
            Assert.IsFalse(effects.Supports(PowerUpType.Barrier));
            effects.For(PowerUpType.Barrier).Apply(); // no lanza excepción
        }

        private void ServeAndLaunch(BallsPresenter balls)
        {
            balls.Serve();
            balls.Tick(rules.relaunchDelay + 0.1f);
        }
    }
}
