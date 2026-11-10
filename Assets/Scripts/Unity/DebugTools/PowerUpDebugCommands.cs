// ============================================================================================
//  HERRAMIENTA DE DEBUG — se puede borrar sin problemas.
//  Para eliminarla: borra la carpeta Assets/Scripts/Unity/DebugTools (con su .meta). No hay que
//  tocar escenas ni ningún otro script: se crea sola al darle a Play y no la referencia nadie.
//  Solo existe en el editor y en Development Builds; en una build normal no se compila.
// ============================================================================================
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Arknoid.Core.Models;
using Arknoid.Core.Models.PowerUps;
using Arknoid.Unity.Installers;
using Arknoid.Unity.Views;
using UnityEngine;

namespace Arknoid.Unity.DebugTools
{
    /// <summary>
    /// Comandos para generar power-ups en un nivel y comprobar que funcionan.
    ///
    ///   F1        Mostrar u ocultar el panel
    ///   1 … 5     Power-up blanco, azul, verde, naranja o rojo encima de la pala (cae y lo recoges)
    ///   0         Los 5 a la vez encima de la pala (prueba de varios seguidos)
    ///   Shift+1…5 El power-up cae desde arriba del todo, con el juego normal
    ///   9         Ráfaga: 10 multibolas seguidos (prueba del límite de bolas)
    ///
    /// Usa el mismo camino que el juego (PowerUpSpawner → PowerUpPresenter → efecto), así que si aquí
    /// funciona, funciona cuando sale de un ladrillo. No gasta el cupo de power-ups del nivel.
    /// </summary>
    public class PowerUpDebugCommands : MonoBehaviour
    {
        private static readonly PowerUpType[] Types =
        {
            PowerUpType.ExtraLife,
            PowerUpType.MultiBallFromStart,
            PowerUpType.MultiBallFromBall,
            PowerUpType.Barrier,
            PowerUpType.BonusScore,
        };

        private static readonly string[] Labels = { "Blanco (+vida)", "Azul (bolas salida)", "Verde (bolas bola)", "Naranja (barrera)", "Rojo (+puntos)" };

        private bool showPanel = true;
        private string lastMessage = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("[Debug] PowerUp Commands");
            DontDestroyOnLoad(go);
            go.AddComponent<PowerUpDebugCommands>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) showPanel = !showPanel;

            bool fromTop = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            for (int i = 0; i < Types.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Spawn(Types[i], fromTop, 0f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha0)) SpawnAll();
            if (Input.GetKeyDown(KeyCode.Alpha9)) MultiBallBurst();
        }

        private void SpawnAll()
        {
            for (int i = 0; i < Types.Length; i++) Spawn(Types[i], false, (i - 2) * 0.5f);
            lastMessage = "Los 5 power-ups a la vez";
        }

        private void MultiBallBurst()
        {
            for (int i = 0; i < 10; i++) Spawn(i % 2 == 0 ? PowerUpType.MultiBallFromStart : PowerUpType.MultiBallFromBall, false, 0f);
            lastMessage = "10 multibolas seguidos";
        }

        private void Spawn(PowerUpType type, bool fromTop, float offsetX)
        {
            var installer = FindObjectOfType<SceneInstaller>();
            if (installer == null || installer.PowerUps == null)
            {
                lastMessage = "Solo funciona dentro de un nivel (Lvl1 / Lvl2)";
                return;
            }

            if (!TryGetSpawnPoint(fromTop, offsetX, out var point))
            {
                lastMessage = "No se encuentra la pala en la escena";
                return;
            }

            var view = installer.PowerUps.Spawn(type, point);
            lastMessage = view != null ? $"Generado: {type}" : $"{type} no está en el PowerUpCatalog";
            Debug.Log("[PowerUpDebug] " + lastMessage);
        }

        /// <summary>Justo encima de la pala (para recogerlo al momento) o en lo alto de la pantalla.</summary>
        private static bool TryGetSpawnPoint(bool fromTop, float offsetX, out WorldPoint point)
        {
            point = default;
            var paddle = FindObjectOfType<PaddleView>();
            if (paddle == null) return false;

            var paddlePos = paddle.transform.position;
            float x = paddlePos.x + offsetX;
            float y;

            if (fromTop && Camera.main != null)
            {
                y = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.9f, 0f)).y;
            }
            else
            {
                var collider = paddle.GetComponent<Collider2D>();
                y = (collider != null ? collider.bounds.max.y : paddlePos.y) + 0.4f;
            }

            point = new WorldPoint(x, y, paddlePos.z);
            return true;
        }

        private void OnGUI()
        {
            if (!showPanel) return;

            GUILayout.BeginArea(new Rect(10, 10, 260, 330), GUI.skin.box);
            GUILayout.Label("<b>Debug power-ups</b>  (F1 ocultar)", new GUIStyle(GUI.skin.label) { richText = true });

            for (int i = 0; i < Types.Length; i++)
            {
                if (GUILayout.Button($"{i + 1}  {Labels[i]}")) Spawn(Types[i], false, 0f);
            }

            if (GUILayout.Button("0  Los 5 a la vez")) SpawnAll();
            if (GUILayout.Button("9  10 multibolas seguidos")) MultiBallBurst();

            GUILayout.Label("Shift + número: cae desde arriba");
            GUILayout.Label(lastMessage);
            GUILayout.EndArea();
        }
    }
}
#endif
