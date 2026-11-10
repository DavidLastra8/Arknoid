using System;

namespace Arknoid.Core.Config
{
    /// <summary>
    /// Valores editables de los power-ups. Se editan en el asset PowerUpCatalog (Assets/Data),
    /// y los efectos los leen en el momento de aplicarse, así que también puedes cambiarlos en Play.
    /// </summary>
    [Serializable]
    public class PowerUpSettings
    {
        /// <summary>Blanco: vidas que da.</summary>
        public int extraLives = 1;

        /// <summary>Azul: bolas nuevas que salen desde la posición inicial de la pelota.</summary>
        public int ballsFromStart = 2;

        /// <summary>Verde: bolas nuevas que salen desde una bola en juego.</summary>
        public int ballsFromBall = 2;

        /// <summary>Naranja: segundos que dura la barrera.</summary>
        public float barrierDuration = 8f;

        /// <summary>Rojo: puntos que suma a la partida actual.</summary>
        public int bonusScore = 100;
    }
}
