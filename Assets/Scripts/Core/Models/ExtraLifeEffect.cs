using Arknoid.Core.Services;

namespace Arknoid.Core.Models
{
    /// <summary>Efecto del power-up actual: +N vidas. Nuevos power-ups = nuevas implementaciones de IPowerUpEffect.</summary>
    public class ExtraLifeEffect : IPowerUpEffect
    {
        private readonly LivesModel lives;
        private readonly int amount;

        public ExtraLifeEffect(LivesModel lives, int amount)
        {
            this.lives = lives;
            this.amount = amount;
        }

        public void Apply() => lives.Add(amount);
    }
}
