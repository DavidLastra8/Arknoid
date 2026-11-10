namespace Arknoid.Core.Services
{
    public enum BallSpawnOrigin
    {
        /// <summary>Posición inicial de salida de la pelota.</summary>
        BallStart,

        /// <summary>Posición de una bola que ya está en juego.</summary>
        ExistingBall,
    }

    public interface IBallSpawner
    {
        void SpawnBalls(int count, BallSpawnOrigin origin);
    }
}
