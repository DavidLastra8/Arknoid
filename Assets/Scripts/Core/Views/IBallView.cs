using System;

namespace Arknoid.Core.Views
{
    public interface IBallView : ITrackable
    {
        /// <summary>Cualquier colisión (se usa para acelerar la pelota).</summary>
        event Action Bounced;
        event Action HitPaddle;
        event Action HitDeathZone;

        float Speed { get; }

        void Launch(float directionX, float directionY, float force);
        void MultiplyVelocity(float factor);
        void ResetToStart();
    }
}
