using System.Collections.Generic;
using Arknoid.Core.Models;
using Arknoid.Core.Services;
using Arknoid.Core.Views;
using Arknoid.Unity.Views;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>
    /// Crea bolas extra clonando la bola de la escena (mismo sprite, física, tamaño y padre).
    /// Las bolas no chocan entre ellas, para que no se aceleren rebotando unas con otras.
    /// </summary>
    public class BallFactory : IBallFactory
    {
        private readonly BallView template;
        private readonly List<Collider2D> ballColliders = new List<Collider2D>();

        public BallFactory(BallView template)
        {
            this.template = template;
            var collider = template.GetComponent<Collider2D>();
            if (collider != null) ballColliders.Add(collider);
        }

        public IBallView Create(WorldPoint position)
        {
            if (template == null) return null;

            var clone = Object.Instantiate(template.gameObject, template.transform.parent);
            clone.name = template.name + " (extra)";
            clone.transform.position = new Vector3(position.X, position.Y, position.Z);
            clone.SetActive(true);

            IgnoreCollisionsWithOtherBalls(clone.GetComponent<Collider2D>());
            return clone.GetComponent<BallView>();
        }

        private void IgnoreCollisionsWithOtherBalls(Collider2D collider)
        {
            if (collider == null) return;

            ballColliders.RemoveAll(c => c == null); // bolas ya destruidas
            foreach (var other in ballColliders) Physics2D.IgnoreCollision(collider, other);
            ballColliders.Add(collider);
        }
    }
}
