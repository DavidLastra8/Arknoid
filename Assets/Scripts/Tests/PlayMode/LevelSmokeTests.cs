using System.Collections;
using System.IO;
using System.Reflection;
using Arknoid.Unity.Installers;
using Arknoid.Unity.Services;
using Arknoid.Unity.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Arknoid.PlayMode.Tests
{
    /// <summary>
    /// Carga los niveles reales y comprueba que arrancan bien con todos los cambios.
    /// El guardado se redirige a un archivo temporal para no pisar tu partida, y al acabar se
    /// vuelve a una escena vacía para no contaminar los demás tests.
    /// </summary>
    public class LevelSmokeTests
    {
        private string savePath;
        private string loadedScene;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savePath = Path.Combine(Application.temporaryCachePath, "arknoid_test.sav");
            if (File.Exists(savePath)) File.Delete(savePath);
            BinaryFileSaveService.PathOverride = savePath;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var empty = SceneManager.CreateScene("SmokeTestCleanup_" + Time.frameCount);
            SceneManager.SetActiveScene(empty);
            if (!string.IsNullOrEmpty(loadedScene)) yield return SceneManager.UnloadSceneAsync(loadedScene);

            // El AudioService sobrevive entre escenas (DontDestroyOnLoad): se elimina a mano.
            foreach (var audio in Object.FindObjectsOfType<AudioService>(true)) Object.Destroy(audio.gameObject);

            Time.timeScale = 1f;
            BinaryFileSaveService.PathOverride = null;
            if (File.Exists(savePath)) File.Delete(savePath);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Level1_StartsCorrectly() => LevelStartsCorrectly("Lvl1");

        [UnityTest]
        public IEnumerator Level2_StartsCorrectly() => LevelStartsCorrectly("Lvl2");

        private IEnumerator LevelStartsCorrectly(string scene)
        {
            loadedScene = scene;
            SceneManager.LoadScene(scene);
            yield return null;
            yield return null;

            var installer = Object.FindObjectOfType<SceneInstaller>();
            Assert.IsNotNull(installer, "Falta el SceneInstaller");
            Assert.IsNotNull(
                typeof(SceneInstaller).GetField("powerUpCatalog", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(installer),
                "El PowerUpCatalog no está asignado en " + scene);

            Assert.AreEqual(1, Object.FindObjectsOfType<BallView>().Length, "Debe empezar con una sola bola");
            Assert.Greater(Object.FindObjectsOfType<BrickView>().Length, 0);

            var barriers = Object.FindObjectsOfType<BarrierView>(true);
            Assert.AreEqual(1, barriers.Length, "Debe crearse la barrera");
            Assert.IsFalse(barriers[0].gameObject.activeSelf, "La barrera empieza oculta");

            var ball = Object.FindObjectOfType<BallView>();
            Vector3 start = ball.transform.position;
            float maxDistance = 0f;

            // 2 s de saque + 1.5 s de juego. Se guarda la distancia máxima por si pierde la bola y vuelve a la salida.
            for (float t = 0f; t < 3.5f; t += Time.deltaTime)
            {
                yield return null;
                if (ball != null) maxDistance = Mathf.Max(maxDistance, Vector3.Distance(start, ball.transform.position));
            }

            Assert.Greater(maxDistance, 0.1f, "La bola debe haberse lanzado");
            Assert.AreEqual(1, Object.FindObjectsOfType<BallView>().Length, "No deben aparecer bolas solas");
            LogAssert.NoUnexpectedReceived();
        }
    }
}
