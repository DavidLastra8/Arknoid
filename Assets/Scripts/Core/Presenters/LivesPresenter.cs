using System;
using Arknoid.Core.Models;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    public class LivesPresenter : IDisposable
    {
        private readonly LivesModel model;
        private readonly ILivesView view;

        public LivesPresenter(LivesModel model, ILivesView view)
        {
            this.model = model;
            this.view = view;

            model.Changed += view.ShowLives;
            view.ShowLives(model.Current);
        }

        public void Dispose()
        {
            model.Changed -= view.ShowLives;
        }
    }
}
