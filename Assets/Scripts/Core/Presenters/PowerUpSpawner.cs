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

            Spawn(selector.Pick(), position);
        }

        /// <summary>
        /// Crea un power-up concreto sin tirar el dado ni gastar el cupo del nivel.
        /// Pasa por el mismo camino que el juego (evento Spawned → presenter + efecto).
        /// Devuelve null si ese tipo no está en el catálogo.
        /// </summary>
        public IPowerUpView Spawn(PowerUpType type, WorldPoint position)
        {
            var powerUp = factory.Create(type, position);
            if (powerUp != null) Spawned?.Invoke(powerUp);
            return powerUp;
        }
    }
}
