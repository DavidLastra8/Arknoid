using System;

namespace Arknoid.Core.Config
{
    /// <summary>
    /// Valores de jugabilidad ajustables desde el Inspector (en el SceneInstaller).
    /// Antes estaban repartidos por BallManager, PlayerMovement y BrickManager.
    /// </summary>
    [Serializable]
    public class GameRules
    {
        public int initialLives = 3;
        public int ballDamage = 1;

        public float launchForce = 300f;
        public float relaunchDelay = 2f;
        public float speedUpFactor = 1.08f;
        public float maxSpeedForSpeedUp = 13f;

        public float paddleMinX = -5f;
        public float paddleMaxX = 5f;

        public float powerUpDropChance = 0.24f;
        public int maxPowerUpsPerLevel = 2;

        /// <summary>Límite de bolas a la vez, para que los power-ups de multibola no se descontrolen.</summary>
        public int maxBallsInPlay = 12;
    }
}
