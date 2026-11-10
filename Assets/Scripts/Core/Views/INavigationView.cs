using System;

namespace Arknoid.Core.Views
{
    /// <summary>Botones de navegación entre escenas (menú, transiciones, game over, "Exit" de la pausa).</summary>
    public interface INavigationView
    {
        event Action NewGameRequested;
        event Action ContinueRequested;
        event Action NextLevelRequested;
        event Action MainMenuRequested;
    }
}
