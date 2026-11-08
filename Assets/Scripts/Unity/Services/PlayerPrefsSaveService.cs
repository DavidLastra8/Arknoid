using Arknoid.Core.Models;
using Arknoid.Core.Services;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>Guardado con PlayerPrefs. Si un día cambias a JSON o a la nube, solo se sustituye esta clase.</summary>
    public class PlayerPrefsSaveService : IRunRepository, IHighScoreRepository
    {
        private const string HighScoreKey = "HighScore";
        private const string RunSceneKey = "Run.Scene";
        private const string RunScoreKey = "Run.Score";
        private const string RunLivesKey = "Run.Lives";

        public int LoadHighScore() => PlayerPrefs.GetInt(HighScoreKey, 0);

        public void SaveHighScore(int score)
        {
            PlayerPrefs.SetInt(HighScoreKey, score);
            PlayerPrefs.Save();
        }

        public bool TryLoad(out RunData run)
        {
            var scene = PlayerPrefs.GetString(RunSceneKey, string.Empty);
            if (string.IsNullOrEmpty(scene))
            {
                run = default;
                return false;
            }

            run = new RunData(scene, PlayerPrefs.GetInt(RunScoreKey, 0), PlayerPrefs.GetInt(RunLivesKey, 0));
            return run.Lives > 0;
        }

        public void Save(RunData run)
        {
            PlayerPrefs.SetString(RunSceneKey, run.SceneName);
            PlayerPrefs.SetInt(RunScoreKey, run.Score);
            PlayerPrefs.SetInt(RunLivesKey, run.Lives);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(RunSceneKey);
            PlayerPrefs.DeleteKey(RunScoreKey);
            PlayerPrefs.DeleteKey(RunLivesKey);
            PlayerPrefs.Save();
        }
    }
}
