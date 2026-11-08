using System;

namespace Arknoid.Core.Views
{
    public interface IPowerUpView
    {
        event Action Collected;
        event Action Missed;

        void Remove();
    }
}
