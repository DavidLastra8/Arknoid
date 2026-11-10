using Arknoid.Core.Services;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>Entrada por teclado con el Input Manager clásico. Para mando o el nuevo Input System, crea otra implementación de IInputService.</summary>
    public class KeyboardInputService : IInputService
    {
        private readonly KeyCode pauseKey;
        private readonly KeyCode autoplayKey;

        public KeyboardInputService(KeyCode pauseKey = KeyCode.Escape, KeyCode autoplayKey = KeyCode.M)
        {
            this.pauseKey = pauseKey;
            this.autoplayKey = autoplayKey;
        }

        public bool PauseTogglePressed => Input.GetKeyDown(pauseKey);
        public bool AutoplayTogglePressed => Input.GetKeyDown(autoplayKey);
    }
}
