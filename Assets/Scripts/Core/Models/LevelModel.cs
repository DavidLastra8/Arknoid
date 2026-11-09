using System;
using System.Collections.Generic;

namespace Arknoid.Core.Models
{
    /// <summary>
    /// Ladrillos del nivel actual. Sustituye a los contadores estáticos Lvl1Brick/Lvl2Brick
    /// y además sabe qué ladrillos quedan, para poder guardarlos.
    /// </summary>
    public class LevelModel
    {
        public event Action BrickDestroyed;
        public event Action Cleared;

        private readonly List<KeyValuePair<string, BrickModel>> bricks = new List<KeyValuePair<string, BrickModel>>();

        public int RemainingBricks { get; private set; }
        public bool IsCleared { get; private set; }

        public void Register(string id, BrickModel brick)
        {
            if (brick == null || brick.IsDestroyed) return;

            bricks.Add(new KeyValuePair<string, BrickModel>(id, brick));
            RemainingBricks++;
        }

        public void NotifyBrickDestroyed()
        {
            if (IsCleared || RemainingBricks <= 0) return;

            RemainingBricks--;
            IsCleared = RemainingBricks == 0;

            BrickDestroyed?.Invoke();
            if (IsCleared) Cleared?.Invoke();
        }

        public List<BrickState> GetRemainingBricks()
        {
            var result = new List<BrickState>(RemainingBricks);
            foreach (var pair in bricks)
            {
                if (!pair.Value.IsDestroyed) result.Add(new BrickState(pair.Key, pair.Value.RemainingHitPoints));
            }

            return result;
        }
    }
}
