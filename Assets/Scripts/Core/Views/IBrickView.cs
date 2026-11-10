using System;
using Arknoid.Core.Models;

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

        /// <summary>Donde aparece el power-up si el ladrillo suelta uno.</summary>
        WorldPoint Position { get; }

        void Break();
    }
}
