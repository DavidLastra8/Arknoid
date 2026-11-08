using System;
using Arknoid.Core.Models;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>
    /// Pausa con Esc o con el botón "Continue". Solo actúa cuando cambia el estado
    /// (antes se llamaba a SetActive y a Time.timeScale en cada frame).
    /// </summary>
    public class PausePresenter : ITickable, IDisposable
    {
        private readonly PauseModel model;
        private readonly IPauseView view;
        private readonly IInputService input;
        private readonly ITimeControl time;

        public PausePresenter(PauseModel model, IPauseView view, IInputService input, ITimeControl time)
        {
            this.model = model;
            this.view = view;
            this.input = input;
            this.time = time;

            model.Changed += Apply;
            view.ResumeRequested += model.Resume;

            Apply(model.IsPaused);
        }

        public void Tick(float deltaTime)
        {
            if (input.PauseTogglePressed) model.Toggle();
        }

        private void Apply(bool paused)
        {
            view.SetPauseMenuVisible(paused);
            time.SetPaused(paused);
        }

        public void Dispose()
        {
            model.Changed -= Apply;
            view.ResumeRequested -= model.Resume;
            time.SetPaused(false);
        }
    }
}
