using System;
using Arknoid.Core.Config;
using Arknoid.Core.Models;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Botones de menú: nueva partida, continuar, siguiente nivel y volver al menú.</summary>
    public class NavigationPresenter : IDisposable
    {
        private readonly INavigationView view;
        private readonly ISceneLoader scenes;
        private readonly IRunRepository runs;
        private readonly LevelSequence levels;
        private readonly GameRules rules;

        public NavigationPresenter(
            INavigationView view,
            ISceneLoader scenes,
            IRunRepository runs,
            LevelSequence levels,
            GameRules rules)
        {
            this.view = view;
            this.scenes = scenes;
            this.runs = runs;
            this.levels = levels;
            this.rules = rules;

            view.NewGameRequested += StartNewGame;
            view.ContinueRequested += ContinueRun;
            view.NextLevelRequested += ContinueRun;
            view.MainMenuRequested += GoToMainMenu;
        }

        private void StartNewGame()
        {
            var run = new RunData(levels.FirstLevel, 0, rules.initialLives);
            runs.Save(run);
            scenes.Load(run.SceneName);
        }

        /// <summary>Carga el nivel guardado. Si no hay partida guardada, empieza una nueva.</summary>
        private void ContinueRun()
        {
            if (runs.TryLoad(out var run))
            {
                scenes.Load(run.SceneName);
            }
            else
            {
                StartNewGame();
            }
        }

        private void GoToMainMenu() => scenes.Load(SceneNames.MainMenu);

        public void Dispose()
        {
            view.NewGameRequested -= StartNewGame;
            view.ContinueRequested -= ContinueRun;
            view.NextLevelRequested -= ContinueRun;
            view.MainMenuRequested -= GoToMainMenu;
        }
    }
}
