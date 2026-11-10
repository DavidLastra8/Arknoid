using System;
using System.IO;
using Arknoid.Core.Models;
using Arknoid.Core.Persistence;
using Arknoid.Core.Services;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>
    /// Guardado en un archivo binario dentro de Application.persistentDataPath
    /// (en Windows: %USERPROFILE%\AppData\LocalLow\&lt;Company&gt;\&lt;Product&gt;\arknoid.sav).
    /// El formato lo define SaveDataBinarySerializer (Core); esta clase solo se ocupa del disco.
    /// </summary>
    public class BinaryFileSaveService : IRunRepository, IHighScoreRepository
    {
        public const string FileName = "arknoid.sav";
        private const string LegacyHighScoreKey = "HighScore";

        private readonly string filePath;
        private SaveData data;

        public BinaryFileSaveService(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>Solo para tests: redirige el guardado a otra ruta para no pisar la partida real.</summary>
        public static string PathOverride { get; set; }

        public static string DefaultPath => PathOverride ?? Path.Combine(Application.persistentDataPath, FileName);

        private SaveData Data => data ?? (data = LoadFromDisk());

        // --- IHighScoreRepository ---
        public int LoadHighScore() => Data.HighScore;

        public void SaveHighScore(int score)
        {
            Data.HighScore = score;
            WriteToDisk();
        }

        // --- IRunRepository ---
        public bool TryLoad(out RunData run)
        {
            run = Data.Run;
            return Data.HasRun && !string.IsNullOrEmpty(run.SceneName) && run.Lives > 0;
        }

        public void Save(RunData run)
        {
            Data.HasRun = true;
            Data.Run = run;
            WriteToDisk();
        }

        public void Clear()
        {
            Data.HasRun = false;
            Data.Run = default;
            WriteToDisk();
        }

        // --- Disco ---
        private SaveData LoadFromDisk()
        {
            if (!File.Exists(filePath)) return CreateFromLegacyPlayerPrefs();

            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return SaveDataBinarySerializer.Read(stream);
                }
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException)
            {
                // Archivo dañado: se aparta (para poder inspeccionarlo) y se empieza de cero en vez de romper el juego.
                Debug.LogWarning($"[BinaryFileSaveService] No se pudo leer '{filePath}': {e.Message}. Se crea un guardado nuevo.");
                TryMove(filePath, filePath + ".corrupt");
                return new SaveData();
            }
        }

        /// <summary>Escribe primero en un .tmp y luego lo sustituye: si se cierra a mitad, el guardado anterior sigue intacto.</summary>
        private void WriteToDisk()
        {
            string tempPath = filePath + ".tmp";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = File.Create(tempPath))
                {
                    SaveDataBinarySerializer.Write(stream, Data);
                }

                if (File.Exists(filePath)) File.Delete(filePath);
                File.Move(tempPath, filePath);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogError($"[BinaryFileSaveService] No se pudo guardar en '{filePath}': {e.Message}");
            }
        }

        /// <summary>Importa una sola vez el récord que guardaba la versión anterior en PlayerPrefs.</summary>
        private static SaveData CreateFromLegacyPlayerPrefs()
        {
            return new SaveData { HighScore = PlayerPrefs.GetInt(LegacyHighScoreKey, 0) };
        }

        private static void TryMove(string from, string to)
        {
            try
            {
                if (File.Exists(to)) File.Delete(to);
                File.Move(from, to);
            }
            catch (IOException)
            {
                // No es crítico.
            }
        }
    }
}
