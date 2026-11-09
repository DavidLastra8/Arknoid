using System;
using System.Collections.Generic;
using Arknoid.Core.Models;
using Arknoid.Core.Presenters;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Tests
{
    // Dobles de prueba. Gracias a que los presenters dependen de interfaces (DIP),
    // se pueden testear sin escenas ni MonoBehaviours.

    internal class FakeRandom : IRandom
    {
        public float NextValue01;
        public float NextRange = 0f;
        public float Value01() => NextValue01;
        public float Range(float min, float max) => NextRange;
    }

    internal class FakeAudio : IAudioService
    {
        public readonly List<GameSound> Played = new List<GameSound>();
        public void Play(GameSound sound) => Played.Add(sound);
    }

    internal class FakeInput : IInputService
    {
        public bool PauseTogglePressed { get; set; }
        public bool AutoplayTogglePressed { get; set; }
    }

    internal class FakeSceneLoader : ISceneLoader
    {
        public string ActiveSceneName { get; set; } = "Test";
        public string LastLoaded;
        public void Load(string sceneName) => LastLoaded = sceneName;
    }

    internal class FakeRunRepository : IRunRepository
    {
        public bool HasRun;
        public RunData Stored;
        public bool Cleared;

        public bool TryLoad(out RunData run)
        {
            run = Stored;
            return HasRun;
        }

        public void Save(RunData run)
        {
            Stored = run;
            HasRun = true;
        }

        public void Clear()
        {
            HasRun = false;
            Cleared = true;
        }
    }

    internal class FakeHighScores : IHighScoreRepository
    {
        public int Saved = -1;
        public int LoadHighScore() => 0;
        public void SaveHighScore(int score) => Saved = score;
    }

    internal class FakeTime : ITimeControl
    {
        public bool Paused;
        public void SetPaused(bool paused) => Paused = paused;
    }

    internal class FakeBallView : IBallView
    {
        public event Action Bounced;
        public event Action HitPaddle;
        public event Action HitDeathZone;

        public float PositionX { get; set; }
        public float Speed { get; set; }
        public int LaunchCount;
        public float LastForce;
        public float VelocityFactor = 1f;
        public int ResetCount;

        public void Launch(float directionX, float directionY, float force)
        {
            LaunchCount++;
            LastForce = force;
        }

        public void MultiplyVelocity(float factor) => VelocityFactor *= factor;
        public void ResetToStart() => ResetCount++;

        public void RaiseBounce() => Bounced?.Invoke();
        public void RaisePaddleHit() => HitPaddle?.Invoke();
        public void RaiseDeath() => HitDeathZone?.Invoke();
    }

    internal class FakePaddleView : IPaddleView
    {
        public event Action<float> SliderMoved;
        public float X;
        public int ResetCount;

        public void SetPositionX(float x) => X = x;
        public void ResetToStart() => ResetCount++;
        public void MoveSlider(float value) => SliderMoved?.Invoke(value);
    }

    internal class FakeBrickView : IBrickView
    {
        public event Action HitByBall;
        public string Id { get; set; } = "brick";
        public int HitPoints { get; set; } = 1;
        public int ScoreValue { get; set; } = 10;
        public bool Broken;
        public int SpawnCount;
        public FakePowerUpView NextPowerUp = new FakePowerUpView();

        public void Break() => Broken = true;

        public IPowerUpView SpawnPowerUp()
        {
            SpawnCount++;
            return NextPowerUp;
        }

        public void Hit() => HitByBall?.Invoke();
    }

    internal class FakePowerUpView : IPowerUpView
    {
        public event Action Collected;
        public event Action Missed;
        public bool Removed;

        public void Remove() => Removed = true;
        public void Collect() => Collected?.Invoke();
        public void Miss() => Missed?.Invoke();
    }

    internal class FakeBallController : IBallController
    {
        public event Action BallLost;
        public int ServeCount;
        public bool Stopped;

        public void Serve() => ServeCount++;
        public void Stop() => Stopped = true;
        public void Lose() => BallLost?.Invoke();
    }

    internal class FakePaddleController : IPaddleController
    {
        public int ResetCount;
        public void ResetPosition() => ResetCount++;
    }
}
