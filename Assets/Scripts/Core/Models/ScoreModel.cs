using System;

namespace Arknoid.Core.Models
{
    /// <summary>Puntuación actual y récord. Solo datos y reglas: no sabe nada de UI ni de guardado.</summary>
    public class ScoreModel
    {
        public event Action<int> CurrentChanged;
        public event Action<int> BestChanged;

        public ScoreModel(int current, int best)
        {
            Current = Math.Max(0, current);
            Best = Math.Max(Math.Max(0, best), Current);
        }

        public int Current { get; private set; }
        public int Best { get; private set; }

        public void Add(int points)
        {
            if (points <= 0) return;

            Current += points;
            CurrentChanged?.Invoke(Current);

            if (Current > Best)
            {
                Best = Current;
                BestChanged?.Invoke(Best);
            }
        }
    }
}
