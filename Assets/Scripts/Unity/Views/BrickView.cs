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

        public event Action HitByBall;

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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<BallView>(out _))
            {
                HitByBall?.Invoke();
            }
        }
    }
}
