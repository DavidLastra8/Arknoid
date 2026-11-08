using System;

namespace Arknoid.Core.Models
{
    /// <summary>
    /// Cuenta los ladrillos que quedan en el nivel actual.
    /// Sustituye a los contadores estáticos Lvl1Brick/Lvl2Brick, que no se reiniciaban entre escenas.
    /// </summary>
    public class LevelModel
    {
        public event Action Cleared;

        public int RemainingBricks { get; private set; }
        public bool IsCleared { get; private set; }

        public void RegisterBrick()
        {
            RemainingBricks++;
        }

        public void NotifyBrickDestroyed()
        {
            if (IsCleared || RemainingBricks <= 0) return;

            RemainingBricks--;
            if (RemainingBricks == 0)
            {
                IsCleared = true;
                Cleared?.Invoke();
            }
        }
    }
}
