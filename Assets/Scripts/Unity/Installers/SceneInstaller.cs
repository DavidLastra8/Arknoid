using System.Collections.Generic;
using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Presenters;
using Arknoid.Core.Services;
using Arknoid.Unity.Services;
using Arknoid.Unity.Views;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arknoid.Unity.Installers
{
    /// <summary>
    /// Composition root de cada escena. Es el único sitio que crea modelos, servicios y presenters
    /// y los conecta con las vistas (inyección por constructor). Sustituye al antiguo GameManager:
    /// está en el mismo GameObject "GameManager" de cada escena, así que no hay que añadir nada.
    ///
    /// - Escenas con ladrillos → monta la partida completa (y restaura el nivel guardado si lo hay).
    /// - Resto (menú, transiciones, game over) → solo UI y navegación.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SceneInstaller : MonoBehaviour
    {
        public enum SceneMode
        {
            Auto,
            Menu,
            Gameplay,
        }

        [Tooltip("Auto: es de juego si hay algún BrickView en la escena.")]
        [SerializeField] private SceneMode mode = SceneMode.Auto;

        [FormerlySerializedAs("uiManager")]
        [SerializeField] private ScreenView screenView;

        [Tooltip("Opcional: si se deja vacío se busca en la escena.")]
        [SerializeField] private BallView ball;

        [Tooltip("Opcional: si se deja vacío se busca en la escena.")]
        [SerializeField] private PaddleView paddle;

        [SerializeField] private GameRules rules = new GameRules();

        private readonly PresenterGroup presenters = new PresenterGroup();
        private RunAutoSaver autoSaver;

        private void Start()
        {
            var scenes = new UnitySceneLoader();
            var save = new BinaryFileSaveService(BinaryFileSaveService.DefaultPath);
            var input = new KeyboardInputService();
            var audio = ResolveAudio();
            var levels = LevelSequence.Default();
            var sceneName = scenes.ActiveSceneName;

            if (screenView == null) screenView = FindObjectOfType<ScreenView>();

            var bricks = FindObjectsOfType<BrickView>();
            bool isGameplay = mode == SceneMode.Gameplay || (mode == SceneMode.Auto && bricks.Length > 0);

            var run = LoadRun(save, sceneName, isGameplay);
            var score = new ScoreModel(run.Score, save.LoadHighScore());
            var lives = new LivesModel(run.Lives);

            if (screenView != null)
            {
                presenters.Add(new ScorePresenter(score, screenView, save));
                presenters.Add(new LivesPresenter(lives, screenView));
                presenters.Add(new NavigationPresenter(screenView, scenes, save, levels, rules));
            }

            if (isGameplay)
            {
                ComposeGameplay(bricks, run.Level, score, lives, audio, input, scenes, save, levels, sceneName);
            }
        }

        private void ComposeGameplay(
            BrickView[] bricks,
            LevelSnapshot savedLevel,
            ScoreModel score,
            LivesModel lives,
            IAudioService audio,
            IInputService input,
            ISceneLoader scenes,
            IRunRepository runs,
            LevelSequence levels,
            string sceneName)
        {
            if (ball == null) ball = FindObjectOfType<BallView>();
            if (paddle == null) paddle = FindObjectOfType<PaddleView>();

            if (ball == null || paddle == null)
            {
                Debug.LogError($"[SceneInstaller] '{sceneName}' necesita un BallView y un PaddleView.", this);
                return;
            }

            // Firma del diseño del nivel: si no coincide con la guardada, el nivel empieza de cero.
            var brickIds = new List<string>(bricks.Length);
            foreach (var brick in bricks) brickIds.Add(brick.Id);
            uint layoutSignature = LevelSnapshot.ComputeLayoutSignature(brickIds);

            if (savedLevel != null && !savedLevel.IsUsableFor(layoutSignature))
            {
                Debug.LogWarning($"[SceneInstaller] El nivel guardado de '{sceneName}' no coincide con la escena actual; se empieza desde cero.");
                savedLevel = null;
            }

            var random = new UnityRandom();
            var level = new LevelModel();

            var ballPresenter = presenters.Add(new BallPresenter(ball, rules, random, audio));
            var paddlePresenter = presenters.Add(new PaddlePresenter(paddle, ball, input, rules));

            if (screenView != null)
            {
                presenters.Add(new PausePresenter(new PauseModel(), screenView, input, new UnityTimeControl()));
            }

            var drops = new PowerUpDropPolicy(
                rules.powerUpDropChance, rules.maxPowerUpsPerLevel, random, savedLevel?.PowerUpsDropped ?? 0);
            var powerUpEffect = new ExtraLifeEffect(lives, rules.powerUpExtraLives);

            foreach (var brick in bricks)
            {
                int hitPoints = brick.HitPoints;

                // Al continuar un nivel a medias: los ladrillos que no están en el guardado ya estaban rotos.
                if (savedLevel != null && !savedLevel.TryGetHitPoints(brick.Id, out hitPoints))
                {
                    brick.Break();
                    continue;
                }

                presenters.Add(new BrickPresenter(
                    brick,
                    new BrickModel(hitPoints, brick.ScoreValue),
                    score,
                    level,
                    drops,
                    audio,
                    rules.ballDamage,
                    powerUp => presenters.Add(new PowerUpPresenter(powerUp, powerUpEffect, audio))));
            }

            var flow = presenters.Add(new GameFlowPresenter(
                lives, score, level, ballPresenter, paddlePresenter, audio, scenes, runs, levels, sceneName));

            autoSaver = presenters.Add(new RunAutoSaver(
                runs, sceneName, layoutSignature, score, lives, level, drops, screenView));

            flow.StartRound();
        }

        /// <summary>
        /// En una escena de juego se usa la partida guardada si es de este nivel; si no (p. ej. al darle
        /// a Play directamente en Lvl2 desde el editor), se empieza una nueva en este nivel.
        /// </summary>
        private RunData LoadRun(IRunRepository runs, string sceneName, bool isGameplay)
        {
            bool hasRun = runs.TryLoad(out var run);

            if (isGameplay && (!hasRun || run.SceneName != sceneName))
            {
                run = new RunData(sceneName, 0, rules.initialLives);
                runs.Save(run);
            }
            else if (!hasRun)
            {
                run = new RunData(string.Empty, 0, rules.initialLives);
            }

            return run;
        }

        private static IAudioService ResolveAudio()
        {
            var audio = FindObjectOfType<AudioService>();
            if (audio != null) return audio;

            Debug.LogWarning("[SceneInstaller] No hay AudioService en la escena: el juego irá sin sonido.");
            return new NullAudioService();
        }

        private void Update()
        {
            presenters.Tick(Time.deltaTime);
        }

        // Guarda el nivel a medias si se cierra el juego (o la app pasa a segundo plano en móvil).
        private void OnApplicationQuit() => autoSaver?.SaveNow();

        private void OnApplicationPause(bool paused)
        {
            if (paused) autoSaver?.SaveNow();
        }

        private void OnDestroy()
        {
            presenters.Dispose();
        }
    }
}
