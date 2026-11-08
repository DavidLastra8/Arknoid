using System;

namespace Arknoid.Core.Models
{
    public class PauseModel
    {
        public event Action<bool> Changed;

        public bool IsPaused { get; private set; }

        public void Toggle() => Set(!IsPaused);
        public void Resume() => Set(false);

        private void Set(bool paused)
        {
            if (IsPaused == paused) return;
            IsPaused = paused;
            Changed?.Invoke(IsPaused);
        }
    }
}
