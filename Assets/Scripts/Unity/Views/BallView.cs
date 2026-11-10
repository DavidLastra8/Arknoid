using System;
using Arknoid.Core.Models;
using Arknoid.Core.Views;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Vista pasiva de una bola: mueve el Rigidbody2D cuando se lo piden y avisa de sus colisiones.
    /// Las reglas (cuándo lanzar, cuánto acelerar, qué pasa al caer) están en BallPresenter y BallsPresenter.
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
        public WorldPoint Position => ToPoint(transform.position);
        public WorldPoint StartPosition => ToPoint(startPosition);

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

        public void BounceUp()
        {
            var velocity = body.velocity;
            if (velocity.y < 0f) body.velocity = new Vector2(velocity.x, -velocity.y);
        }

        public void SetActive(bool active) => gameObject.SetActive(active);

        public void Remove() => Destroy(gameObject);

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

        private static WorldPoint ToPoint(Vector3 v) => new WorldPoint(v.x, v.y, v.z);
    }
}
