using System;
using Arknoid.Core.Models.PowerUps;

namespace Arknoid.Core.Views
{
    public interface IPowerUpView
    {
        event Action Collected;
        event Action Missed;

        PowerUpType Type { get; }

        void Remove();
    }
}
