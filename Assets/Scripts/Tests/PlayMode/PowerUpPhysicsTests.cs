#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Presenters;
using Arknoid.Core.Services;
using Arknoid.Core.Views;
using Arknoid.Unity.Services;
using Arknoid.Unity.Views;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Arknoid.PlayMode.Tests
{
    /// <summary>
    /// Tests con FÍSICA REAL (PlayMode). Montan un mini-nivel por código (paredes, pala y DeathCollider)
    /// con los prefabs reales del juego (bola Circle y power-ups), y lo conectan con los presenters reales.
    ///
    /// Geometría del mini-nivel (unidades del mundo):
    ///   paredes en x = ±10.5 y techo en y = 6.5 · pala en (0, -3.5), 3 de ancho · DeathCollider en y = -5, 20 de ancho
    /// Las bolas que hay que tirar al DeathCollider se lanzan en x = 6, lejos de la pala.
    /// </summary>
    public class PowerUpPhysicsTests
    {
        private const string BallPrefabPath = "Assets/Prefabs/Circle.prefab";
        private const float SafeX = 6f;

        private readonly List<GameObject> created = new List<GameObject>();
        private PresenterGroup presenters;
        private GameRules rules;
        private PowerUpSettings settings;
        private IAudioService audio;
        private BallView mainBall;
        private BallsPresenter balls;
        private BarrierPresenter barrier;
        private CountingBallFactory ballFactory;
        private PowerUpEffects effects;
        private Collider2D deathCollider;
        private LivesModel lives;
        private ScoreModel score;
        private int livesLost;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            presenters = new PresenterGroup();
            rules = new GameRules();
            settings = new PowerUpSettings();
            audio = new NullAudioService();
            lives = new LivesModel(3);
            score = new ScoreModel(0, 0);
            livesLost = 0;

            deathCollider = CreateBox("DeathCollider", new Vector2(0f, -5f), new Vector2(20f, 1f));
            deathCollider.gameObject.tag = "DeathCollider";
            CreateBox("WallLeft", new Vector2(-10.5f, 0f), new Vector2(1f, 14f));
            CreateBox("WallRight", new Vector2(10.5f, 0f), new Vector2(1f, 14f));
            CreateBox("Ceiling", new Vector2(0f, 6.5f), new Vector2(22f, 1f));
            CreateBox("Paddle", new Vector2(0f, -3.5f), new Vector2(3f, 0.3f)).gameObject.AddComponent<PaddleView>();

            mainBall = Track(Object.Instantiate(Load<BallView>(BallPrefabPath), Vector3.zero, Quaternion.identity));

            yield return new WaitForFixedUpdate(); // que los colliders tengan bounds

            var random = new UnityRandom();
            ballFactory = new CountingBallFactory(new BallFactory(mainBall));
            balls = presenters.Add(new BallsPresenter(mainBall, ballFactory, rules, random, audio));
            balls.BallLost += () =>
            {
                // Lo mismo que hace GameFlowPresenter al perder una vida.
                livesLost++;
                balls.Serve();
            };

            barrier = presenters.Add(new BarrierPresenter(
                BarrierView.Create(deathCollider.bounds, 0.3f, 0.05f, new Color(1f, 0.55f, 0.1f, 1f))));

            effects = new PowerUpEffects()
                .Register(PowerUpType.ExtraLife, new ExtraLifeEffect(lives, settings))
                .Register(PowerUpType.MultiBallFromStart,
                    new SpawnBallsEffect(balls, BallSpawnOrigin.BallStart, () => settings.ballsFromStart))
                .Register(PowerUpType.MultiBallFromBall,
                    new SpawnBallsEffect(balls, BallSpawnOrigin.ExistingBall, () => settings.ballsFromBall))
                .Register(PowerUpType.Barrier, new BarrierEffect(barrier, settings))
                .Register(PowerUpType.BonusScore, new BonusScoreEffect(score, settings));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            presenters.Dispose();
            foreach (var ball in Object.FindObjectsOfType<BallView>(true)) Object.Destroy(ball.gameObject);
            foreach (var powerUp in Object.FindObjectsOfType<PowerUpView>(true)) Object.Destroy(powerUp.gameObject);
            foreach (var view in Object.FindObjectsOfType<BarrierView>(true)) Object.Destroy(view.gameObject);
            foreach (var go in created) if (go != null) Object.Destroy(go);
            created.Clear();
            yield return null;
        }

        // ---------------- DeathCollider: que nada se quede atrapado ----------------

        [UnityTest]
        public IEnumerator LastBall_TouchingDeathCollider_IsResetToStart_AndNotStuck()
        {
            yield return ServeAndLaunch();
            Throw(mainBall, new Vector2(SafeX, -3.5f), new Vector2(0f, -8f));

            yield return Run(1f, () => livesLost > 0);
            Assert.AreEqual(1, livesLost, "Debe perder una vida");

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.Less(Vector2.Distance(mainBall.transform.position, Vector2.zero), 0.01f, "Vuelve a la salida");
            Assert.IsFalse(mainBall.GetComponent<Collider2D>().IsTouching(deathCollider), "No sigue pegada al DeathCollider");

            yield return Run(rules.relaunchDelay + 0.5f);
            Assert.Greater(Body(mainBall).velocity.magnitude, 0.1f, "Se vuelve a lanzar");
            Assert.AreEqual(1, livesLost, "No se pierde otra vida de golpe");
        }

        [UnityTest]
        public IEnumerator ExtraBalls_TouchingDeathCollider_AreDestroyed_WithoutLosingLives()
        {
            yield return ServeAndLaunch();
            Freeze(mainBall, new Vector2(0f, 2f));
            balls.SpawnBalls(3, BallSpawnOrigin.BallStart);
            yield return null;

            var extras = Extras();
            Assert.AreEqual(3, extras.Count);
            for (int i = 0; i < extras.Count; i++) Throw(extras[i], new Vector2(SafeX + i, -3.5f), new Vector2(0f, -8f));

            yield return Run(1f, () => Object.FindObjectsOfType<BallView>().Length == 1);

            Assert.AreEqual(1, Object.FindObjectsOfType<BallView>().Length, "Las extra se destruyen al caer");
            Assert.AreEqual(0, livesLost);
            Assert.AreEqual(1, balls.BallsInPlay);
        }

        [UnityTest]
        public IEnumerator MainBallLost_WhileExtrasAlive_IsHidden_AndOnlyTheLastBallCostsALife()
        {
            yield return ServeAndLaunch();
            balls.SpawnBalls(1, BallSpawnOrigin.BallStart);
            yield return null;
            var extra = Extras()[0];
            Freeze(extra, new Vector2(0f, 2f));

            Throw(mainBall, new Vector2(SafeX, -3.5f), new Vector2(0f, -8f));
            yield return Run(1f, () => !mainBall.gameObject.activeSelf);
            Assert.IsFalse(mainBall.gameObject.activeSelf, "La principal se esconde");
            Assert.AreEqual(0, livesLost);

            Throw(extra, new Vector2(SafeX, -3.5f), new Vector2(0f, -8f));
            yield return Run(1f, () => livesLost > 0);
            Assert.AreEqual(1, livesLost);
            Assert.IsTrue(mainBall.gameObject.activeSelf, "Vuelve para el nuevo saque");
        }

        // ---------------- Barrera ----------------

        [UnityTest]
        public IEnumerator BallAtMaxSpeed_BouncesOnBarrier_AndNeverReachesDeathCollider()
        {
            yield return BarrierStopsBallAt(rules.maxSpeedForSpeedUp * rules.speedUpFactor);
        }

        [UnityTest]
        public IEnumerator BallAtDoubleMaxSpeed_StillDoesNotTunnelThroughBarrier()
        {
            // Margen de seguridad: la bola nunca llega a esta velocidad en el juego.
            yield return BarrierStopsBallAt(2f * rules.maxSpeedForSpeedUp * rules.speedUpFactor);
        }

        [UnityTest]
        public IEnumerator PowerUp_FallsThroughActiveBarrier_AndIsRemovedAtDeathCollider()
        {
            barrier.Activate(10f);
            var powerUp = SpawnPowerUp("PowerUpOrange", new Vector2(SafeX, -2f));
            bool collected = false;
            powerUp.Collected += () => collected = true;
            var go = powerUp.gameObject;

            yield return Run(3f, () => go == null);

            Assert.IsTrue(go == null, "No se queda encima de la barrera ni del DeathCollider");
            Assert.IsFalse(collected);
        }

        [UnityTest]
        public IEnumerator PowerUp_ThatNeverTouchesAnything_IsRemovedAfterItsLifetime()
        {
            var powerUp = SpawnPowerUp("PowerUp", new Vector2(40f, 0f)); // fuera del nivel
            typeof(PowerUpView)
                .GetField("maxLifetime", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(powerUp, 0.5f);
            var go = powerUp.gameObject;

            yield return Run(2f, () => go == null);

            Assert.IsTrue(go == null);
        }

        // ---------------- Varios power-ups seguidos ----------------

        [UnityTest]
        public IEnumerator FivePowerUpsCaughtAtOnce_AllApplyWithoutErrors()
        {
            yield return ServeAndLaunch();
            Freeze(mainBall, new Vector2(0f, 2f));

            string[] prefabs = { "PowerUp", "PowerUpBlue", "PowerUpGreen", "PowerUpOrange", "PowerUpRed" };
            int collected = 0;
            for (int i = 0; i < prefabs.Length; i++)
            {
                var powerUp = SpawnPowerUp(prefabs[i], new Vector2(-1.2f + 0.6f * i, -2.9f));
                powerUp.Collected += () => collected++;
            }

            yield return Run(3f, () => collected == prefabs.Length);

            Assert.AreEqual(prefabs.Length, collected, "La pala recoge los 5");
            Assert.AreEqual(4, lives.Current);
            Assert.AreEqual(settings.bonusScore, score.Current);
            Assert.IsTrue(barrier.IsActive);
            Assert.AreEqual(settings.ballsFromStart + settings.ballsFromBall, ballFactory.Created);
            Assert.AreEqual(0, Object.FindObjectsOfType<PowerUpView>().Length, "No quedan power-ups en escena");
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator BallsDoNotCollideWithEachOther_NorWithPowerUps()
        {
            yield return ServeAndLaunch();
            balls.SpawnBalls(2, BallSpawnOrigin.BallStart);
            yield return null;

            var colliders = new List<Collider2D> { mainBall.GetComponent<Collider2D>() };
            foreach (var extra in Extras()) colliders.Add(extra.GetComponent<Collider2D>());

            for (int i = 0; i < colliders.Count; i++)
            for (int j = i + 1; j < colliders.Count; j++)
                Assert.IsTrue(Physics2D.GetIgnoreCollision(colliders[i], colliders[j]), $"Bolas {i} y {j} chocan");

            var powerUpLayer = Load<PowerUpView>("Assets/Prefabs/PowerUp.prefab").gameObject.layer;
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(mainBall.gameObject.layer, powerUpLayer),
                "Si la bola choca con los power-ups, los desvía y se acelera");
        }

        // ---------------- Utilidades ----------------

        private IEnumerator BarrierStopsBallAt(float speed)
        {
            yield return ServeAndLaunch();
            barrier.Activate(5f);
            Throw(mainBall, new Vector2(SafeX, -2f), new Vector2(0f, -speed));

            bool wentUp = false;
            yield return Run(1f, () => wentUp |= Body(mainBall).velocity.y > 0f);

            Assert.IsTrue(wentUp, $"A {speed:0.0} u/s la bola debe rebotar en la barrera");
            Assert.AreEqual(0, livesLost, "No debe llegar al DeathCollider");
        }

        private IEnumerator ServeAndLaunch()
        {
            balls.Serve();
            yield return Run(rules.relaunchDelay + 0.1f);
        }

        /// <summary>Avanza frames actualizando los presenters (como SceneInstaller.Update) hasta que se cumpla la condición.</summary>
        private IEnumerator Run(float seconds, Func<bool> until = null)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                yield return null;
                presenters.Tick(Time.deltaTime);
                elapsed += Time.deltaTime;
                if (until != null && until()) yield break;
            }
        }

        private PowerUpView SpawnPowerUp(string prefabName, Vector2 position)
        {
            var view = Object.Instantiate(Load<PowerUpView>($"Assets/Prefabs/{prefabName}.prefab"), position, Quaternion.identity);
            presenters.Add(new PowerUpPresenter(view, effects.For(view.Type), audio));
            return view;
        }

        private List<BallView> Extras()
        {
            var list = new List<BallView>(Object.FindObjectsOfType<BallView>());
            list.Remove(mainBall);
            return list;
        }

        private static Rigidbody2D Body(BallView ball) => ball.GetComponent<Rigidbody2D>();

        private static void Throw(BallView ball, Vector2 position, Vector2 velocity)
        {
            ball.transform.position = position;
            var body = Body(ball);
            body.position = position;
            body.velocity = velocity;
        }

        private static void Freeze(BallView ball, Vector2 position) => Throw(ball, position, Vector2.zero);

        private Collider2D CreateBox(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            created.Add(go);
            return box;
        }

        private T Track<T>(T component) where T : Component
        {
            created.Add(component.gameObject);
            return component;
        }

        private static T Load<T>(string path) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, "No se encuentra " + path);
            return prefab.GetComponent<T>();
        }

        /// <summary>Cuenta cuántas bolas extra se crean.</summary>
        private class CountingBallFactory : IBallFactory
        {
            private readonly IBallFactory inner;
            public int Created;

            public CountingBallFactory(IBallFactory inner) => this.inner = inner;

            public IBallView Create(WorldPoint position)
            {
                Created++;
                return inner.Create(position);
            }
        }
    }
}
#endif
