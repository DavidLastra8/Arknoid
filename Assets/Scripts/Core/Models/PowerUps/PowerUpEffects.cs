using System.Collections.Generic;
using Arknoid.Core.Services;

namespace Arknoid.Core.Models.PowerUps
{
    /// <summary>
    /// Qué efecto tiene cada tipo de power-up. Para añadir uno nuevo: un valor en PowerUpType,
    /// una clase IPowerUpEffect, registrarla aquí desde el installer y un prefab en el catálogo.
    /// </summary>
    public class PowerUpEffects
    {
        private static readonly IPowerUpEffect None = new NoEffect();
        private readonly Dictionary<PowerUpType, IPowerUpEffect> effects = new Dictionary<PowerUpType, IPowerUpEffect>();

        public PowerUpEffects Register(PowerUpType type, IPowerUpEffect effect)
        {
            effects[type] = effect;
            return this;
        }

        public bool Supports(PowerUpType type) => effects.ContainsKey(type);

        public IPowerUpEffect For(PowerUpType type) => effects.TryGetValue(type, out var effect) ? effect : None;

        private sealed class NoEffect : IPowerUpEffect
        {
            public void Apply() { }
        }
    }
}
