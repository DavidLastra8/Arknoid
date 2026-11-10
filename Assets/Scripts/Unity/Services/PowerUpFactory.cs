using System.Collections.Generic;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Services;
using Arknoid.Core.Views;
using Arknoid.Unity.Views;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>Instancia el prefab del catálogo que corresponde a cada tipo de power-up.</summary>
    public class PowerUpFactory : IPowerUpFactory
    {
        private readonly Dictionary<PowerUpType, PowerUpView> prefabsByType = new Dictionary<PowerUpType, PowerUpView>();
        private readonly List<PowerUpType> availableTypes = new List<PowerUpType>();

        public PowerUpFactory(IEnumerable<PowerUpView> prefabs, System.Predicate<PowerUpType> isSupported)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab == null) continue;

                if (prefabsByType.ContainsKey(prefab.Type))
                {
                    Debug.LogWarning($"[PowerUpFactory] Hay dos prefabs de tipo {prefab.Type}; se usa '{prefabsByType[prefab.Type].name}'.");
                    continue;
                }

                if (!isSupported(prefab.Type))
                {
                    Debug.LogWarning($"[PowerUpFactory] El tipo {prefab.Type} no se puede usar en esta escena; se ignora '{prefab.name}'.");
                    continue;
                }

                prefabsByType.Add(prefab.Type, prefab);
                availableTypes.Add(prefab.Type);
            }
        }

        public IReadOnlyList<PowerUpType> AvailableTypes => availableTypes;

        public IPowerUpView Create(PowerUpType type, WorldPoint position)
        {
            if (!prefabsByType.TryGetValue(type, out var prefab)) return null;

            return Object.Instantiate(prefab, new Vector3(position.X, position.Y, position.Z), Quaternion.identity);
        }
    }
}
