using System;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>
    /// Azul y verde: añade bolas. La misma clase sirve para los dos; solo cambian el origen y
    /// de qué ajuste sale el número de bolas.
    /// </summary>
    public class SpawnBallsEffect : IPowerUpEffect
    {
        private readonly IBallSpawner spawner;
        private readonly BallSpawnOrigin origin;
        private readonly Func<int> count;

        public SpawnBallsEffect(IBallSpawner spawner, BallSpawnOrigin origin, Func<int> count)
        {
            this.spawner = spawner;
            this.origin = origin;
            this.count = count;
        }

        public void Apply() => spawner.SpawnBalls(count(), origin);
    }
}
