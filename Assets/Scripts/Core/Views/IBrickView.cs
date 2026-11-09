using System;

namespace Arknoid.Core.Views
{
    public interface IBrickView
    {
        event Action HitByBall;

        /// <summary>Identificador estable dentro de la escena (para guardar qué ladrillos quedan).</summary>
        string Id { get; }

        /// <summary>Datos de diseño del prefab (vida y puntos), usados para crear el BrickModel.</summary>
        int HitPoints { get; }
        int ScoreValue { get; }

        void Break();

        /// <summary>Instancia el power-up en la posición del ladrillo. Devuelve null si no tiene prefab.</summary>
        IPowerUpView SpawnPowerUp();
    }
}
