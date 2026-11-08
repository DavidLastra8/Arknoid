using System;
using System.Collections.Generic;
using Arknoid.Core.Services;

namespace Arknoid.Core.Presenters
{
    /// <summary>Guarda los presenters de una escena: los actualiza cada frame y los libera al salir.</summary>
    public class PresenterGroup : ITickable, IDisposable
    {
        private readonly List<object> items = new List<object>();
        private readonly List<ITickable> tickBuffer = new List<ITickable>();

        public T Add<T>(T presenter)
        {
            items.Add(presenter);
            return presenter;
        }

        public void Tick(float deltaTime)
        {
            // Se copia por si un presenter añade otros durante el Tick.
            tickBuffer.Clear();
            foreach (var item in items)
            {
                if (item is ITickable tickable) tickBuffer.Add(tickable);
            }

            foreach (var tickable in tickBuffer) tickable.Tick(deltaTime);
        }

        public void Dispose()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                (items[i] as IDisposable)?.Dispose();
            }

            items.Clear();
        }
    }
}
