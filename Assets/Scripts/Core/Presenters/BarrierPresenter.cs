using System;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>Barrera inferior temporal: mientras está activa, las bolas que la tocan rebotan hacia arriba.</summary>
    public class BarrierPresenter : IBarrierController, ITickable, IDisposable
    {
        private readonly IBarrierView view;
        private float remaining;

        public BarrierPresenter(IBarrierView view)
        {
            this.view = view;
            view.BallEntered += OnBallEntered;
            view.SetVisible(false);
        }

        public bool IsActive => remaining > 0f;
        public float RemainingTime => Math.Max(0f, remaining);

        public void Activate(float duration)
        {
            if (duration <= 0f) return;

            // Coger otro mientras está activa renueva la duración (no se acumula).
            remaining = Math.Max(remaining, duration);
            view.SetVisible(true);
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive) return;

            remaining -= deltaTime;
            if (!IsActive) view.SetVisible(false);
        }

        private void OnBallEntered(IBallView ball)
        {
            if (IsActive) ball.BounceUp();
        }

        public void Dispose()
        {
            view.BallEntered -= OnBallEntered;
        }
    }
}
