using Arknoid.Core.Config;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>Blanco: +N vidas.</summary>
    public class ExtraLifeEffect : IPowerUpEffect
    {
        private readonly LivesModel lives;
        private readonly PowerUpSettings settings;

        public ExtraLifeEffect(LivesModel lives, PowerUpSettings settings)
        {
            this.lives = lives;
            this.settings = settings;
        }

        public void Apply() => lives.Add(settings.extraLives);
    }
}
