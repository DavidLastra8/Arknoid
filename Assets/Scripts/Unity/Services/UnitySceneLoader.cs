using Arknoid.Core.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arknoid.Unity.Services
{
    public class UnitySceneLoader : ISceneLoader
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public void Load(string sceneName)
        {
            // Por si se sale desde el menú de pausa: la siguiente escena no debe empezar congelada.
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
