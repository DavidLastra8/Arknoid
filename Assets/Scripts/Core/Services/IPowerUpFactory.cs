using System.Collections.Generic;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Views;

namespace Arknoid.Core.Services
{
    public interface IPowerUpFactory
    {
        /// <summary>Tipos que tienen prefab asignado en el catálogo.</summary>
        IReadOnlyList<PowerUpType> AvailableTypes { get; }

        IPowerUpView Create(PowerUpType type, WorldPoint position);
    }
}
