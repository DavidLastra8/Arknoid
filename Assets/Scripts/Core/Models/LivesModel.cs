using System;

namespace Arknoid.Core.Models
{
    public class LivesModel
    {
        public event Action<int> Changed;

        public LivesModel(int initial)
        {
            Current = Math.Max(0, initial);
        }

        public int Current { get; private set; }
        public bool IsDepleted => Current <= 0;

        public void LoseOne()
        {
            if (IsDepleted) return;
            Current--;
            Changed?.Invoke(Current);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Current += amount;
            Changed?.Invoke(Current);
        }
    }
}
