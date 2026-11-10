using System;
using Arknoid.Core.Config;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Mueve la pala con el slider o, en modo automático (tecla M), siguiendo a la pelota.</summary>
    public class PaddlePresenter : IPaddleController, ITickable, IDisposable
    {
        private readonly IPaddleView view;
        private readonly ITrackable target;
        private readonly IInputService input;
        private readonly GameRules rules;

        public PaddlePresenter(IPaddleView view, ITrackable target, IInputService input, GameRules rules)
        {
            this.view = view;
            this.target = target;
            this.input = input;
            this.rules = rules;

            view.SliderMoved += OnSliderMoved;
        }

        public bool IsAutomatic { get; private set; }

        public void ResetPosition() => view.ResetToStart();

        public void Tick(float deltaTime)
        {
            if (input.AutoplayTogglePressed)
            {
                IsAutomatic = !IsAutomatic;
            }

            if (IsAutomatic)
            {
                view.SetPositionX(target.PositionX);
            }
        }

        private void OnSliderMoved(float normalized)
        {
            float t = Math.Max(0f, Math.Min(1f, normalized));
            view.SetPositionX(rules.paddleMinX + (rules.paddleMaxX - rules.paddleMinX) * t);
        }

        public void Dispose()
        {
            view.SliderMoved -= OnSliderMoved;
        }
    }
}
