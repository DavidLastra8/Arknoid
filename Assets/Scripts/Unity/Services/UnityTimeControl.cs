using Arknoid.Core.Services;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    public class UnityTimeControl : ITimeControl
    {
        public void SetPaused(bool paused) => Time.timeScale = paused ? 0f : 1f;
    }
}
