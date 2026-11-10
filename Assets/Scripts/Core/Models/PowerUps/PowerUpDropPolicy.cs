using System;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>Decide si un ladrillo roto suelta power-up (probabilidad + máximo por nivel).</summary>
    public class PowerUpDropPolicy
    {
        private readonly float chance;
        private readonly int maxDrops;
        private readonly IRandom random;

        /// <param name="dropsSoFar">Power-ups ya soltados en este nivel (al restaurar una partida guardada).</param>
        public PowerUpDropPolicy(float chance, int maxDrops, IRandom random, int dropsSoFar = 0)
        {
            this.chance = chance;
            this.maxDrops = maxDrops;
            this.random = random;
            DropsSoFar = Math.Max(0, dropsSoFar);
        }

        public int DropsSoFar { get; private set; }

        public bool TryDrop()
        {
            if (DropsSoFar >= maxDrops) return false;
            if (random.Value01() >= chance) return false;

            DropsSoFar++;
            return true;
        }
    }
}
