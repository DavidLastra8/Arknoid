using System;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Core.Views;
using UnityEngine;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Vista del power-up: cae y avisa si lo recoge la pala o si se pierde por abajo.
    /// Todos los power-ups usan este script; cada prefab solo cambia el color y el Type.
    /// </summary>
    public class PowerUpView : MonoBehaviour, IPowerUpView
    {
        [SerializeField] private PowerUpType type = PowerUpType.ExtraLife;

        // Antes bajaba 0.1 unidades por frame (dependía de los FPS). 6 u/s equivale a 60 FPS.
        [SerializeField] private float fallSpeed = 6f;
        [SerializeField] private string missZoneTag = "DeathCollider";

        public event Action Collected;
        public event Action Missed;

        public PowerUpType Type => type;

        private void Update()
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var other = collision.gameObject;

            if (other.TryGetComponent<PaddleView>(out _))
            {
                Collected?.Invoke();
            }
            else if (other.CompareTag(missZoneTag))
            {
                Missed?.Invoke();
            }
        }
    }
}
