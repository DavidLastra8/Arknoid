using System;

namespace Arknoid.Core.Views
{
    public interface IPaddleView
    {
        /// <summary>El jugador ha movido el slider. Valor normalizado 0..1.</summary>
        event Action<float> SliderMoved;

        void SetPositionX(float x);
        void ResetToStart();
    }
}
