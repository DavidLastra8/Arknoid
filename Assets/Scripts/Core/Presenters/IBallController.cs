using System;

namespace Arknoid.Core.Presenters
{
    /// <summary>Lo que el flujo de partida necesita de la pelota (permite testear GameFlowPresenter con un fake).</summary>
    public interface IBallController
    {
        event Action BallLost;

        /// <summary>Recoloca la pelota y la lanza tras el retardo configurado.</summary>
        void Serve();

        void Stop();
    }
}
