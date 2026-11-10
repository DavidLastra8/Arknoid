using System;
using Arknoid.Core.Views;
using UnityEngine;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Barrera naranja. Es un trigger: las bolas rebotan (BarrierPresenter les invierte la velocidad)
    /// y los power-ups la atraviesan, así que no se quedan atascados encima.
    /// Se crea por código justo encima del DeathCollider, con su mismo ancho.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BarrierView : MonoBehaviour, IBarrierView
    {
        private static Sprite whiteSprite;

        public event Action<IBallView> BallEntered;

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        private void OnTriggerEnter2D(Collider2D other) => Notify(other);

        // Por si la barrera se activa con una bola ya dentro.
        private void OnTriggerStay2D(Collider2D other) => Notify(other);

        private void Notify(Collider2D other)
        {
            if (other.TryGetComponent<BallView>(out var ball)) BallEntered?.Invoke(ball);
        }

        /// <summary>Crea la barrera encima de <paramref name="deathZone"/> (límites en el mundo del DeathCollider).</summary>
        public static BarrierView Create(Bounds deathZone, float thickness, float gap, Color color)
        {
            var go = new GameObject("Barrier");
            go.transform.position = new Vector3(
                deathZone.center.x,
                deathZone.max.y + gap + thickness * 0.5f,
                deathZone.center.z);
            go.transform.localScale = new Vector3(deathZone.size.x, thickness, 1f);

            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = WhiteSprite();
            sprite.color = color;
            sprite.sortingOrder = 10;

            // Se añade antes que BarrierView, así RequireComponent ya lo encuentra.
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = Vector2.one;

            return go.AddComponent<BarrierView>();
        }

        /// <summary>Sprite blanco de 1×1 unidad generado en tiempo de ejecución (no hace falta ningún asset).</summary>
        private static Sprite WhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;

            var texture = Texture2D.whiteTexture;
            whiteSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
            return whiteSprite;
        }
    }
}
