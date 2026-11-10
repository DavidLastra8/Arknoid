using System;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>
    /// Guarda la partida a medias (puntos, vidas y ladrillos que quedan) cuando se rompe un ladrillo,
    /// cuando cambian las vidas, al salir al menú desde la pausa y al cerrar el juego (SaveNow()).
    /// Si el nivel ya está superado o se ha perdido, no guarda: de eso se encarga GameFlowPresenter.
    /// </summary>
    public class RunAutoSaver : IDisposable
    {
        private readonly IRunRepository runs;
        private readonly string sceneName;
        private readonly uint layoutSignature;
        private readonly ScoreModel score;
        private readonly LivesModel lives;
        private readonly LevelModel level;
        private readonly PowerUpDropPolicy drops;
        private readonly INavigationView navigation;

        public RunAutoSaver(
            IRunRepository runs,
            string sceneName,
            uint layoutSignature,
            ScoreModel score,
            LivesModel lives,
            LevelModel level,
            PowerUpDropPolicy drops,
            INavigationView navigation = null)
        {
            this.runs = runs;
            this.sceneName = sceneName;
            this.layoutSignature = layoutSignature;
            this.score = score;
            this.lives = lives;
            this.level = level;
            this.drops = drops;
            this.navigation = navigation;

            level.BrickDestroyed += SaveNow;
            lives.Changed += OnLivesChanged;
            if (navigation != null) navigation.MainMenuRequested += SaveNow;
        }

        public void SaveNow()
        {
            if (level.IsCleared || lives.IsDepleted) return;

            var snapshot = new LevelSnapshot(layoutSignature, drops.DropsSoFar, level.GetRemainingBricks());
            runs.Save(new RunData(sceneName, score.Current, lives.Current, snapshot));
        }

        private void OnLivesChanged(int _) => SaveNow();

        public void Dispose()
        {
            level.BrickDestroyed -= SaveNow;
            lives.Changed -= OnLivesChanged;
            if (navigation != null) navigation.MainMenuRequested -= SaveNow;
        }
    }
}
