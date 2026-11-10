using System;
using System.Collections.Generic;
using System.IO;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Unity.Services;
using Arknoid.Unity.Views;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Arknoid.Unity.EditorTests
{
    /// <summary>Comprueba que los assets de los power-ups están bien configurados (sin entrar en Play).</summary>
    public class PowerUpAssetTests
    {
        private const string CatalogPath = "Assets/Data/PowerUpCatalog.asset";

        private static PowerUpCatalog Catalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PowerUpCatalog>(CatalogPath);
            Assert.IsNotNull(catalog, "No existe " + CatalogPath);
            return catalog;
        }

        [Test]
        public void Catalog_HasExactlyOnePrefabPerPowerUpType()
        {
            var seen = new HashSet<PowerUpType>();
            foreach (var prefab in Catalog().Prefabs)
            {
                Assert.IsNotNull(prefab, "Hay un hueco vacío en la lista del catálogo");
                Assert.IsTrue(seen.Add(prefab.Type), "Tipo repetido: " + prefab.Type);
            }

            foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
            {
                Assert.IsTrue(seen.Contains(type), "Falta en el catálogo: " + type);
            }
        }

        [Test]
        public void PowerUps_HaveTheSameShape_ButDifferentColors()
        {
            var prefabs = Catalog().Prefabs;
            var reference = prefabs[0].GetComponent<SpriteRenderer>();
            var colors = new HashSet<Color>();

            foreach (var prefab in prefabs)
            {
                var sprite = prefab.GetComponent<SpriteRenderer>();
                Assert.AreEqual(reference.sprite, sprite.sprite, prefab.name + ": forma distinta");
                Assert.AreEqual(prefabs[0].transform.localScale, prefab.transform.localScale, prefab.name + ": tamaño distinto");
                Assert.AreEqual(prefabs[0].gameObject.layer, prefab.gameObject.layer, prefab.name + ": capa distinta");
                Assert.IsTrue(colors.Add(sprite.color), prefab.name + ": color repetido");
            }
        }

        [Test]
        public void Settings_HaveSensibleDefaults()
        {
            var s = Catalog().Settings;
            Assert.Greater(s.extraLives, 0);
            Assert.Greater(s.ballsFromStart, 0);
            Assert.Greater(s.ballsFromBall, 0);
            Assert.Greater(s.barrierDuration, 0f);
            Assert.Greater(s.bonusScore, 0);
        }

        [Test]
        public void Physics_BallsDoNotCollideWithPowerUps()
        {
            var ball = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Circle.prefab");
            foreach (var prefab in Catalog().Prefabs)
            {
                Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(ball.layer, prefab.gameObject.layer),
                    $"La capa de la bola ({LayerMask.LayerToName(ball.layer)}) choca con la de {prefab.name}");
            }
        }

        [Test]
        public void Ball_UsesContinuousCollisionDetection()
        {
            var ball = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Circle.prefab");
            Assert.AreEqual(CollisionDetectionMode2D.Continuous, ball.GetComponent<Rigidbody2D>().collisionDetectionMode,
                "Con 'Discrete' la bola puede atravesar la barrera (o un ladrillo) a mucha velocidad");
        }

        [TestCase("Assets/Scenes/Lvl1.unity")]
        [TestCase("Assets/Scenes/Lvl2.unity")]
        public void LevelScenes_HaveTheCatalogAssigned(string scenePath)
        {
            string guid = AssetDatabase.AssetPathToGUID(CatalogPath);
            string yaml = File.ReadAllText(scenePath);
            Assert.IsTrue(yaml.Contains("powerUpCatalog: {fileID: 11400000, guid: " + guid),
                "El SceneInstaller de " + scenePath + " no tiene el PowerUpCatalog asignado");
        }
    }
}
