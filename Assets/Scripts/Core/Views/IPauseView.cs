using System;

namespace Arknoid.Core.Views
{
    public interface IPauseView
    {
        event Action ResumeRequested;
        void SetPauseMenuVisible(bool visible);
    }
}
