namespace Arknoid.Core.Models
{
    /// <summary>Partida en curso que se guarda entre sesiones (para "Load"/"Continue").</summary>
    public readonly struct RunData
    {
        public RunData(string sceneName, int score, int lives, LevelSnapshot level = null)
        {
            SceneName = sceneName;
            Score = score;
            Lives = lives;
            Level = level;
        }

        public string SceneName { get; }
        public int Score { get; }
        public int Lives { get; }

        /// <summary>Estado del nivel a medias. null = el nivel empieza desde cero.</summary>
        public LevelSnapshot Level { get; }
    }
}
