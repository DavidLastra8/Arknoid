using Arknoid.Core.Models;
using NUnit.Framework;

namespace Arknoid.Core.Tests
{
    public class ModelTests
    {
        [Test]
        public void Score_Add_UpdatesCurrentAndBest()
        {
            var score = new ScoreModel(0, 15);
            int bestEvents = 0;
            score.BestChanged += _ => bestEvents++;

            score.Add(10);
            Assert.AreEqual(10, score.Current);
            Assert.AreEqual(15, score.Best);
            Assert.AreEqual(0, bestEvents);

            score.Add(10);
            Assert.AreEqual(20, score.Best);
            Assert.AreEqual(1, bestEvents);
        }

        [Test]
        public void Lives_LoseOne_StopsAtZero()
        {
            var lives = new LivesModel(1);
            lives.LoseOne();
            lives.LoseOne();

            Assert.AreEqual(0, lives.Current);
            Assert.IsTrue(lives.IsDepleted);
        }

        [Test]
        public void Brick_TakeHit_DestroyedWhenHitPointsReachZero()
        {
            var brick = new BrickModel(2, 20);

            Assert.IsFalse(brick.TakeHit(1));
            Assert.IsTrue(brick.TakeHit(1));
            Assert.IsFalse(brick.TakeHit(1), "Un ladrillo roto no se vuelve a romper");
        }

        [Test]
        public void Level_Cleared_FiresOnceWhenLastBrickIsDestroyed()
        {
            var level = new LevelModel();
            int cleared = 0;
            level.Cleared += () => cleared++;
            level.Register("a", new BrickModel(1, 10));
            level.Register("b", new BrickModel(1, 10));

            level.NotifyBrickDestroyed();
            Assert.AreEqual(0, cleared);

            level.NotifyBrickDestroyed();
            level.NotifyBrickDestroyed();
            Assert.AreEqual(1, cleared);
        }

        [Test]
        public void DropPolicy_RespectsChanceAndMaximum()
        {
            var random = new FakeRandom { NextValue01 = 0.1f };
            var policy = new PowerUpDropPolicy(0.24f, 2, random);

            Assert.IsTrue(policy.TryDrop());
            Assert.IsTrue(policy.TryDrop());
            Assert.IsFalse(policy.TryDrop(), "Máximo 2 por nivel");

            var unlucky = new PowerUpDropPolicy(0.24f, 2, new FakeRandom { NextValue01 = 0.5f });
            Assert.IsFalse(unlucky.TryDrop());
        }

        [Test]
        public void Pause_ToggleAndResume()
        {
            var pause = new PauseModel();
            pause.Toggle();
            Assert.IsTrue(pause.IsPaused);
            pause.Resume();
            Assert.IsFalse(pause.IsPaused);
        }
    }
}
