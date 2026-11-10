using System;
using Arknoid.Core.Views;
using UnityEngine;

namespace Arknoid.Unity.Views
{
    /// <summary>
    /// Barrera naranja. Es un trigger: las bolas rebotan (BarrierPresenter les invierte la velocidad)
    /// y los power-ups la atraviesan, así que no se quedan atascados encima.
    ///
    /// Se coloca justo encima del DeathCollider y con su mismo ancho. IMPORTANTE: el tamaño se calcula
    /// al activarse y se reajusta cada frame mientras está activa, no al crearla. El DeathCollider está
    /// dentro de un Canvas que Unity escala en tiempo de ejecución (en la escena tiene escala 0), así que
    /// en el Start() de la escena sus medidas todavía no son las reales. Además, así se adapta si cambia
    /// el tamaño de la ventana.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BarrierView : MonoBehaviour, IBarrierView
    {
        private static Sprite whiteSprite;

        private Collider2D deathZone;
        private float thickness;
        private float gap;

        public event Action<IBallView> BallEntered;

        public void SetVisible(bool visible)
        {
            if (visible) FitAboveDeathZone();
            gameObject.SetActive(visible);
        }

        private void LateUpdate() => FitAboveDeathZone();

        private void OnTriggerEnter2D(Collider2D other) => Notify(other);

        // Por si la barrera se activa con una bola ya dentro.
        private void OnTriggerStay2D(Collider2D other) => Notify(other);

        private void Notify(Collider2D other)
        {
            if (other.TryGetComponent<BallView>(out var ball)) BallEntered?.Invoke(ball);
        }

        /// <summary>Coloca la barrera encima del DeathCollider, con su mismo ancho, usando su posición actual.</summary>
        public void FitAboveDeathZone()
        {
            if (deathZone == null) return;

            var zone = WorldRect(deathZone);
            transform.position = new Vector3(
                (zone.xMin + zone.xMax) * 0.5f,
                zone.yMax + gap + thickness * 0.5f,
                deathZone.transform.position.z);
            transform.localScale = new Vector3(zone.width, thickness, 1f);
        }

        /// <summary>
        /// Rectángulo del collider en el mundo calculado desde su transform, que está siempre al día.
        /// Collider2D.bounds viene de la simulación física y solo se actualiza en el siguiente paso de física.
        /// </summary>
        private static Rect WorldRect(Collider2D collider)
        {
            if (collider is BoxCollider2D box)
            {
                var half = box.size * 0.5f;
                var t = box.transform;
                Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 max = new Vector2(float.MinValue, float.MinValue);

                for (int i = 0; i < 4; i++)
                {
                    var local = box.offset + new Vector2(i % 2 == 0 ? -half.x : half.x, i < 2 ? -half.y : half.y);
                    Vector2 world = t.TransformPoint(local);
                    min = Vector2.Min(min, world);
                    max = Vector2.Max(max, world);
                }

                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }

            Physics2D.SyncTransforms();
            var b = collider.bounds;
            return Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
        }

        /// <summary>Crea la barrera (oculta hasta que se active) asociada a <paramref name="deathZone"/>.</summary>
        public static BarrierView Create(Collider2D deathZone, float thickness, float gap, Color color)
        {
            var go = new GameObject("Barrier");

            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = WhiteSprite();
            sprite.color = color;
            sprite.sortingOrder = 10;

            // Se añade antes que BarrierView, así RequireComponent ya lo encuentra.
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = Vector2.one;

            var view = go.AddComponent<BarrierView>();
            view.deathZone = deathZone;
            view.thickness = thickness;
            view.gap = gap;
            view.FitAboveDeathZone();
            return view;
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
