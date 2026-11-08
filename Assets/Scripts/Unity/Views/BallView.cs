using System;
using Arknoid.Core.Views;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Vista pasiva de la pelota: mueve el Rigidbody2D cuando se lo piden y avisa de sus colisiones.
    /// Las reglas (cuándo lanzar, cuánto acelerar, qué pasa al caer) están en BallPresenter.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallView : MonoBehaviour, IBallView
    {
        [FormerlySerializedAs("rigidbody")]
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private string deathZoneTag = "DeathCollider";

        private Vector3 startPosition;

        public event Action Bounced;
        public event Action HitPaddle;
        public event Action HitDeathZone;

        public float PositionX => transform.position.x;
        public float Speed => body.velocity.magnitude;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            startPosition = transform.position;
        }

        public void Launch(float directionX, float directionY, float force)
        {
            body.AddForce(new Vector2(directionX, directionY) * force);
        }

        public void MultiplyVelocity(float factor)
        {
            body.velocity *= factor;
        }

        public void ResetToStart()
        {
            body.velocity = Vector2.zero;
            transform.position = startPosition;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var other = collision.gameObject;

            if (other.CompareTag(deathZoneTag))
            {
                HitDeathZone?.Invoke();
            }
            else if (other.TryGetComponent<PaddleView>(out _))
            {
                HitPaddle?.Invoke();
            }

            Bounced?.Invoke();
        }
    }
}
