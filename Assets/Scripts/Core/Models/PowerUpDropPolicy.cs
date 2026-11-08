using Arknoid.Core.Services;

namespace Arknoid.Core.Models
{
    /// <summary>Decide si un ladrillo roto suelta power-up (probabilidad + máximo por nivel).</summary>
    public class PowerUpDropPolicy
    {
        private readonly float chance;
        private readonly int maxDrops;
        private readonly IRandom random;

        public PowerUpDropPolicy(float chance, int maxDrops, IRandom random)
        {
            this.chance = chance;
            this.maxDrops = maxDrops;
            this.random = random;
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
