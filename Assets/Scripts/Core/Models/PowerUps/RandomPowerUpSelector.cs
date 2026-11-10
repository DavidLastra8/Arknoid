using System;
using System.Collections.Generic;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>Elige al azar, con la misma probabilidad, uno de los tipos de power-up disponibles.</summary>
    public class RandomPowerUpSelector
    {
        private readonly List<PowerUpType> types;
        private readonly IRandom random;

        public RandomPowerUpSelector(IEnumerable<PowerUpType> types, IRandom random)
        {
            this.types = new List<PowerUpType>(types);
            this.random = random;
        }

        public bool HasAny => types.Count > 0;

        public PowerUpType Pick()
        {
            if (types.Count == 0) throw new InvalidOperationException("No hay power-ups disponibles.");

            int index = (int)(random.Value01() * types.Count);
            return types[Math.Min(Math.Max(index, 0), types.Count - 1)];
        }
    }
}
