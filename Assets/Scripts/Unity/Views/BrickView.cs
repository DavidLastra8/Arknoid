using System;
using Arknoid.Core.Views;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Vista de un ladrillo. Guarda los datos de diseño del prefab (vida, puntos, power-up)
    /// y avisa cuando la pelota lo golpea. Ya no usa tags distintos por nivel ("Ball"/"Ball2").
    /// </summary>
    public class BrickView : MonoBehaviour, IBrickView
    {
        [FormerlySerializedAs("brickHP")]
        [SerializeField] private int hitPoints = 1;

        [FormerlySerializedAs("brickScore")]
        [SerializeField] private int scoreValue = 10;

        [FormerlySerializedAs("powerUp")]
        [SerializeField] private GameObject powerUpPrefab;

        [Tooltip("Opcional. Si se deja vacío, el ID se genera a partir de la posición en la jerarquía.")]
        [SerializeField] private string persistentId;

        private string cachedId;

        public event Action HitByBall;

        public string Id => cachedId ?? (cachedId = string.IsNullOrEmpty(persistentId) ? BuildHierarchyId(transform) : persistentId);

        public int HitPoints => hitPoints;
        public int ScoreValue => scoreValue;

        public void Break()
        {
            Destroy(gameObject);
        }

        public IPowerUpView SpawnPowerUp()
        {
            if (powerUpPrefab == null) return null;

            var instance = Instantiate(powerUpPrefab, transform.position, transform.rotation);
            return instance.GetComponent<PowerUpView>();
        }

        /// <summary>
        /// Ruta de índices de hermano desde la raíz, p. ej. "6/1/14". Es estable mientras no reordenes
        /// la jerarquía; si lo haces, cambia la firma del nivel y el guardado del nivel se ignora sin romper nada.
        /// </summary>
        private static string BuildHierarchyId(Transform node)
        {
            var path = new System.Text.StringBuilder();
            for (var current = node; current != null; current = current.parent)
            {
                path.Insert(0, current.GetSiblingIndex());
                if (current.parent != null) path.Insert(0, '/');
            }

            return path.ToString();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<BallView>(out _))
            {
                HitByBall?.Invoke();
            }
        }
    }
}
