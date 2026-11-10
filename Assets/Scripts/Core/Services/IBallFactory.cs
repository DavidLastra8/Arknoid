using Arknoid.Core.Models;
using Arknoid.Core.Views;

namespace Arknoid.Core.Services
{
    public interface IBallFactory
    {
        /// <summary>Crea una bola extra en la posición indicada. Devuelve null si no se puede.</summary>
        IBallView Create(WorldPoint position);
    }
}
