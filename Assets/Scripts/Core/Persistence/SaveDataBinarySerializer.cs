using System.Collections.Generic;
using System.IO;
using System.Text;
using Arknoid.Core.Models;

namespace Arknoid.Core.Persistence
{
    /// <summary>
    /// Convierte SaveData a binario y viceversa con BinaryWriter/BinaryReader.
    /// (No se usa BinaryFormatter: está obsoleto y es inseguro con datos manipulados.)
    ///
    /// Formato (little-endian):
    ///   uint   magic "ARKN"
    ///   int    versión
    ///   int    récord
    ///   bool   hay partida
    ///     string escena, int puntos, int vidas
    ///     bool   hay nivel a medias
    ///       uint firma del nivel, int power-ups soltados, int nº ladrillos
    ///       [string id, int vida] × nº ladrillos
    ///
    /// Si algún día cambias el formato, sube CurrentVersion y lee las versiones antiguas en Read().
    /// </summary>
    public static class SaveDataBinarySerializer
    {
        public const uint Magic = 0x4E4B5241; // "ARKN" en ASCII, little-endian
        public const int CurrentVersion = 1;
        private const int MaxBricks = 4096;

        public static void Write(Stream stream, SaveData data)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(Magic);
                writer.Write(CurrentVersion);
                writer.Write(data.HighScore);
                writer.Write(data.HasRun);

                if (!data.HasRun) return;

                var run = data.Run;
                writer.Write(run.SceneName ?? string.Empty);
                writer.Write(run.Score);
                writer.Write(run.Lives);
                writer.Write(run.Level != null);

                if (run.Level == null) return;

                writer.Write(run.Level.LayoutSignature);
                writer.Write(run.Level.PowerUpsDropped);
                writer.Write(run.Level.Bricks.Count);
                foreach (var brick in run.Level.Bricks)
                {
                    writer.Write(brick.Id ?? string.Empty);
                    writer.Write(brick.HitPoints);
                }
            }
        }

        /// <exception cref="InvalidDataException">El archivo no es un guardado válido.</exception>
        /// <exception cref="EndOfStreamException">El archivo está cortado.</exception>
        public static SaveData Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                if (reader.ReadUInt32() != Magic) throw new InvalidDataException("No es un archivo de guardado de Arknoid.");

                int version = reader.ReadInt32();
                if (version < 1 || version > CurrentVersion)
                {
                    throw new InvalidDataException($"Versión de guardado no soportada: {version}.");
                }

                var data = new SaveData
                {
                    HighScore = reader.ReadInt32(),
                    HasRun = reader.ReadBoolean(),
                };

                if (!data.HasRun) return data;

                string scene = reader.ReadString();
                int score = reader.ReadInt32();
                int lives = reader.ReadInt32();
                LevelSnapshot level = null;

                if (reader.ReadBoolean())
                {
                    uint signature = reader.ReadUInt32();
                    int powerUps = reader.ReadInt32();
                    int count = reader.ReadInt32();
                    if (count < 0 || count > MaxBricks) throw new InvalidDataException($"Número de ladrillos inválido: {count}.");

                    var bricks = new List<BrickState>(count);
                    for (int i = 0; i < count; i++)
                    {
                        bricks.Add(new BrickState(reader.ReadString(), reader.ReadInt32()));
                    }

                    level = new LevelSnapshot(signature, powerUps, bricks);
                }

                data.Run = new RunData(scene, score, lives, level);
                return data;
            }
        }
    }
}
