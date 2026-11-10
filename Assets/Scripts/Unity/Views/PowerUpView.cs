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

        [Tooltip("Segundos máximos en pantalla. Si no ha tocado ni la pala ni el DeathCollider (p. ej. cae fuera de él), se pierde.")]
        [SerializeField] private float maxLifetime = 10f;

        private float age;
        private bool finished;

        public event Action Collected;
        public event Action Missed;

        public PowerUpType Type => type;

        private void Update()
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            age += Time.deltaTime;
            if (age >= maxLifetime) RaiseMissed();
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var other = collision.gameObject;

            if (finished) return;

            if (other.TryGetComponent<PaddleView>(out _))
            {
                finished = true;
                Collected?.Invoke();
            }
            else if (other.CompareTag(missZoneTag))
            {
                RaiseMissed();
            }
        }

        private void RaiseMissed()
        {
            if (finished) return;
            finished = true;

            // Sin presenter (p. ej. instanciado a mano) se destruye solo, para no quedarse para siempre.
            if (Missed == null) Destroy(gameObject);
            else Missed.Invoke();
        }
    }
}
