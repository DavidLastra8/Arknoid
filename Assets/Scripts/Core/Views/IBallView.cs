using System;
using Arknoid.Core.Models;

namespace Arknoid.Core.Views
{
    public interface IBallView : ITrackable
    {
        /// <summary>Cualquier colisión (se usa para acelerar la pelota).</summary>
        event Action Bounced;
        event Action HitPaddle;
        event Action HitDeathZone;

        float Speed { get; }
        WorldPoint Position { get; }

        /// <summary>Posición inicial de salida (donde estaba la bola al cargar la escena).</summary>
        WorldPoint StartPosition { get; }

        void Launch(float directionX, float directionY, float force);
        void MultiplyVelocity(float factor);
        void ResetToStart();

        /// <summary>Si va hacia abajo, invierte la velocidad vertical (rebote en la barrera).</summary>
        void BounceUp();

        void SetActive(bool active);

        /// <summary>Destruye la bola (solo las extra; la principal se reutiliza en cada saque).</summary>
        void Remove();
    }
}
