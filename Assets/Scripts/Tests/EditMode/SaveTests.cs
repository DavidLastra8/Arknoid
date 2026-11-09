using System;
using System.IO;
using Arknoid.Core.Models;
using Arknoid.Core.Persistence;
using Arknoid.Core.Presenters;
using NUnit.Framework;

namespace Arknoid.Core.Tests
{
    public class SaveTests
    {
        [Test]
        public void Serializer_RoundTripKeepsRunAndRemainingBricks()
        {
            var level = new LevelSnapshot(1234u, 1, new[] { new BrickState("0/2/5", 3), new BrickState("0/2/9", 1) });
            var original = new SaveData { HighScore = 900, HasRun = true, Run = new RunData("Lvl2", 450, 2, level) };

            var copy = RoundTrip(original);

            Assert.AreEqual(900, copy.HighScore);
            Assert.IsTrue(copy.HasRun);
            Assert.AreEqual("Lvl2", copy.Run.SceneName);
            Assert.AreEqual(450, copy.Run.Score);
            Assert.AreEqual(2, copy.Run.Lives);
            Assert.AreEqual(1234u, copy.Run.Level.LayoutSignature);
            Assert.AreEqual(1, copy.Run.Level.PowerUpsDropped);
            Assert.AreEqual(2, copy.Run.Level.Bricks.Count);
            Assert.IsTrue(copy.Run.Level.TryGetHitPoints("0/2/5", out int hp));
            Assert.AreEqual(3, hp);
        }

        [Test]
        public void Serializer_RunWithoutLevelStateAndNoRun()
        {
            var fresh = RoundTrip(new SaveData { HasRun = true, Run = new RunData("Lvl1", 0, 3) });
            Assert.IsTrue(fresh.Run.Level == null);

            var empty = RoundTrip(new SaveData { HighScore = 5 });
            Assert.IsFalse(empty.HasRun);
            Assert.AreEqual(5, empty.HighScore);
        }

        [Test]
        public void Serializer_RejectsFilesThatAreNotSaves()
        {
            Assert.IsTrue(Throws<InvalidDataException>(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));

            var bytes = Bytes(new SaveData { HasRun = true, Run = new RunData("Lvl1", 10, 3) });
            Array.Resize(ref bytes, bytes.Length - 3); // archivo cortado
            Assert.IsTrue(Throws<EndOfStreamException>(bytes));
        }

        [Test]
        public void Snapshot_SignatureDependsOnLayoutNotOrder()
        {
            uint a = LevelSnapshot.ComputeLayoutSignature(new[] { "0/1", "0/2", "0/3" });
            uint b = LevelSnapshot.ComputeLayoutSignature(new[] { "0/3", "0/1", "0/2" });
            uint c = LevelSnapshot.ComputeLayoutSignature(new[] { "0/1", "0/2" });

            Assert.AreEqual(a, b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(new LevelSnapshot(a, 0, new BrickState[0]).IsUsableFor(a), "Sin ladrillos no se restaura");
        }

        [Test]
        public void AutoSaver_SavesRemainingBricksWithDamageWhenABrickBreaks()
        {
            var runs = new FakeRunRepository();
            var score = new ScoreModel(0, 0);
            var lives = new LivesModel(3);
            var level = new LevelModel();
            var damaged = new BrickModel(3, 10);
            var doomed = new BrickModel(1, 10);
            level.Register("damaged", damaged);
            level.Register("doomed", doomed);
            level.Register("intact", new BrickModel(2, 10));
            var drops = new PowerUpDropPolicy(0f, 2, new FakeRandom(), dropsSoFar: 1);
            new RunAutoSaver(runs, "Lvl1", 42u, score, lives, level, drops);

            damaged.TakeHit(1);
            score.Add(30);
            doomed.TakeHit(1);
            level.NotifyBrickDestroyed();

            Assert.AreEqual("Lvl1", runs.Stored.SceneName);
            Assert.AreEqual(30, runs.Stored.Score);
            Assert.AreEqual(42u, runs.Stored.Level.LayoutSignature);
            Assert.AreEqual(1, runs.Stored.Level.PowerUpsDropped);
            Assert.AreEqual(2, runs.Stored.Level.Bricks.Count);
            Assert.IsFalse(runs.Stored.Level.TryGetHitPoints("doomed", out _));
            Assert.IsTrue(runs.Stored.Level.TryGetHitPoints("damaged", out int hp));
            Assert.AreEqual(2, hp);
        }

        [Test]
        public void AutoSaver_DoesNotOverwriteWhenLevelIsClearedOrGameIsLost()
        {
            var runs = new FakeRunRepository();
            var lives = new LivesModel(1);
            var level = new LevelModel();
            level.Register("last", new BrickModel(1, 10));
            new RunAutoSaver(runs, "Lvl1", 1u, new ScoreModel(0, 0), lives, level,
                new PowerUpDropPolicy(0f, 0, new FakeRandom()));

            lives.LoseOne();
            Assert.IsFalse(runs.HasRun, "Sin vidas: GameFlowPresenter borra la partida");

            var level2 = new LevelModel();
            level2.Register("last", new BrickModel(1, 10));
            var runs2 = new FakeRunRepository();
            new RunAutoSaver(runs2, "Lvl1", 1u, new ScoreModel(0, 0), new LivesModel(3), level2,
                new PowerUpDropPolicy(0f, 0, new FakeRandom()));
            level2.NotifyBrickDestroyed();
            Assert.IsFalse(runs2.HasRun, "Nivel superado: GameFlowPresenter guarda el siguiente nivel");
        }

        private static SaveData RoundTrip(SaveData data)
        {
            using (var stream = new MemoryStream(Bytes(data)))
            {
                return SaveDataBinarySerializer.Read(stream);
            }
        }

        private static byte[] Bytes(SaveData data)
        {
            using (var stream = new MemoryStream())
            {
                SaveDataBinarySerializer.Write(stream, data);
                return stream.ToArray();
            }
        }

        private static bool Throws<T>(byte[] bytes) where T : Exception
        {
            try
            {
                using (var stream = new MemoryStream(bytes)) SaveDataBinarySerializer.Read(stream);
                return false;
            }
            catch (T)
            {
                return true;
            }
        }
    }
}
