using Arknoid.Core.Models;

namespace Arknoid.Core.Persistence
{
    /// <summary>Todo lo que se escribe en el archivo de guardado.</summary>
    public class SaveData
    {
        public int HighScore { get; set; }
        public bool HasRun { get; set; }
        public RunData Run { get; set; }
    }
}
