using System;

namespace Arknoid.Core.Views
{
    public interface IBarrierView
    {
        event Action<IBallView> BallEntered;

        void SetVisible(bool visible);
    }
}
