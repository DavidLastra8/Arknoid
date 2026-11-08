using System;
using Arknoid.Core.Models;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Muestra la puntuación y guarda el récord cuando se supera.</summary>
    public class ScorePresenter : IDisposable
    {
        private readonly ScoreModel model;
        private readonly IScoreView view;
        private readonly IHighScoreRepository highScores;

        public ScorePresenter(ScoreModel model, IScoreView view, IHighScoreRepository highScores)
        {
            this.model = model;
            this.view = view;
            this.highScores = highScores;

            model.CurrentChanged += view.ShowScore;
            model.BestChanged += OnBestChanged;

            view.ShowScore(model.Current);
            view.ShowBestScore(model.Best);
        }

        private void OnBestChanged(int best)
        {
            view.ShowBestScore(best);
            highScores.SaveHighScore(best);
        }

        public void Dispose()
        {
            model.CurrentChanged -= view.ShowScore;
            model.BestChanged -= OnBestChanged;
        }
    }
}
