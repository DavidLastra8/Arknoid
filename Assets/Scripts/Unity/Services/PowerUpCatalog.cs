using System.Collections.Generic;
using Arknoid.Core.Config;
using Arknoid.Unity.Views;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>
    /// Asset con todos los power-ups del juego y sus valores editables (Assets/Data/PowerUpCatalog).
    /// Cuando se rompe un ladrillo y toca soltar power-up, se elige uno al azar de esta lista.
    /// Para añadir uno: crea su prefab (con PowerUpView y su Type) y arrástralo a "Prefabs".
    /// </summary>
    [CreateAssetMenu(menuName = "Arknoid/Power-Up Catalog", fileName = "PowerUpCatalog")]
    public class PowerUpCatalog : ScriptableObject
    {
        [Tooltip("Vidas extra (blanco), nº de bolas (azul/verde), duración de la barrera (naranja) y puntos extra (rojo).")]
        [SerializeField] private PowerUpSettings settings = new PowerUpSettings();

        [Tooltip("Power-ups que pueden salir de los ladrillos (todos con la misma probabilidad).")]
        [SerializeField] private List<PowerUpView> prefabs = new List<PowerUpView>();

        [Header("Barrera (naranja)")]
        [SerializeField] private Color barrierColor = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField] private float barrierThickness = 0.3f;
        [Tooltip("Separación entre la barrera y el DeathCollider, en unidades del mundo.")]
        [SerializeField] private float barrierGap = 0.05f;

        public PowerUpSettings Settings => settings;
        public IReadOnlyList<PowerUpView> Prefabs => prefabs;
        public Color BarrierColor => barrierColor;
        public float BarrierThickness => barrierThickness;
        public float BarrierGap => barrierGap;
    }
}
