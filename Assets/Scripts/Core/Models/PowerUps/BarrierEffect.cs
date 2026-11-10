using Arknoid.Core.Config;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>Naranja: activa la barrera inferior durante PowerUpSettings.barrierDuration segundos.</summary>
    public class BarrierEffect : IPowerUpEffect
    {
        private readonly IBarrierController barrier;
        private readonly PowerUpSettings settings;

        public BarrierEffect(IBarrierController barrier, PowerUpSettings settings)
        {
            this.barrier = barrier;
            this.settings = settings;
        }

        public void Apply() => barrier.Activate(settings.barrierDuration);
    }
}
