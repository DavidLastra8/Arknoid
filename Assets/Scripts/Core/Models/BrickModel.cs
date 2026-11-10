using System;

namespace Arknoid.Core.Models
{
    public class BrickModel
    {
        public BrickModel(int hitPoints, int scoreValue)
        {
            RemainingHitPoints = Math.Max(1, hitPoints);
            ScoreValue = Math.Max(0, scoreValue);
        }

        public int RemainingHitPoints { get; private set; }
        public int ScoreValue { get; }
        public bool IsDestroyed => RemainingHitPoints <= 0;

        /// <summary>Aplica daño. Devuelve true si el ladrillo se rompe con este golpe.</summary>
        public bool TakeHit(int damage)
        {
            if (IsDestroyed) return false;
            RemainingHitPoints -= Math.Max(1, damage);
            return IsDestroyed;
        }
    }
}
