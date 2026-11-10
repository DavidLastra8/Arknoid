using System;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Services;
using Arknoid.Core.Views;

namespace Arknoid.Core.Presenters
{
    /// <summary>
    /// Cuando se rompe un ladrillo: tira el dado (PowerUpDropPolicy), y si toca, elige un tipo al azar
    /// entre todos los disponibles (RandomPowerUpSelector) y lo crea (IPowerUpFactory).
    /// </summary>
    public class PowerUpSpawner
    {
        public event Action<IPowerUpView> Spawned;

        private readonly PowerUpDropPolicy policy;
        private readonly RandomPowerUpSelector selector;
        private readonly IPowerUpFactory factory;

        public PowerUpSpawner(PowerUpDropPolicy policy, RandomPowerUpSelector selector, IPowerUpFactory factory)
        {
            this.policy = policy;
            this.selector = selector;
            this.factory = factory;
        }

        public void TrySpawnAt(WorldPoint position)
        {
            if (!selector.HasAny || !policy.TryDrop()) return;

            var powerUp = factory.Create(selector.Pick(), position);
            if (powerUp != null) Spawned?.Invoke(powerUp);
        }
    }
}
