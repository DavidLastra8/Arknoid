using Arknoid.Core.Config;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>Rojo: suma puntos a la partida actual (y actualiza el récord si se supera).</summary>
    public class BonusScoreEffect : IPowerUpEffect
    {
        private readonly ScoreModel score;
        private readonly PowerUpSettings settings;

        public BonusScoreEffect(ScoreModel score, PowerUpSettings settings)
        {
            this.score = score;
            this.settings = settings;
        }

        public void Apply() => score.Add(settings.bonusScore);
    }
}
