namespace Arknoid.Core.Models
{
    /// <summary>Partida en curso que se guarda entre escenas (para "Continue").</summary>
    public readonly struct RunData
    {
        public RunData(string sceneName, int score, int lives)
        {
            SceneName = sceneName;
            Score = score;
            Lives = lives;
        }

        public string SceneName { get; }
        public int Score { get; }
        public int Lives { get; }
    }
}
