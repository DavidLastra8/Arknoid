using System;
using System.Collections.Generic;

namespace Arknoid.Core.Models
{
    /// <summary>
    /// Foto del nivel a medias: ladrillos que quedan (con su vida) y power-ups ya soltados.
    /// La "firma" identifica el diseño del nivel: si cambias los ladrillos en el editor,
    /// la firma deja de coincidir y la foto se descarta en vez de borrar ladrillos equivocados.
    /// </summary>
    public sealed class LevelSnapshot
    {
        private readonly List<BrickState> bricks;
        private Dictionary<string, int> lookup;

        public LevelSnapshot(uint layoutSignature, int powerUpsDropped, IEnumerable<BrickState> bricks)
        {
            LayoutSignature = layoutSignature;
            PowerUpsDropped = Math.Max(0, powerUpsDropped);
            this.bricks = new List<BrickState>(bricks);
        }

        public uint LayoutSignature { get; }
        public int PowerUpsDropped { get; }
        public IReadOnlyList<BrickState> Bricks => bricks;

        /// <summary>Solo se puede restaurar sobre el mismo diseño de nivel y si quedan ladrillos.</summary>
        public bool IsUsableFor(uint layoutSignature) => LayoutSignature == layoutSignature && bricks.Count > 0;

        public bool TryGetHitPoints(string brickId, out int hitPoints)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var b in bricks) lookup[b.Id] = b.HitPoints;
            }

            return lookup.TryGetValue(brickId, out hitPoints);
        }

        /// <summary>
        /// Hash FNV-1a de los IDs ordenados. No se usa string.GetHashCode() porque no
        /// garantiza el mismo valor entre ejecuciones o plataformas.
        /// </summary>
        public static uint ComputeLayoutSignature(IEnumerable<string> brickIds)
        {
            var ids = new List<string>(brickIds);
            ids.Sort(StringComparer.Ordinal);

            uint hash = 2166136261;
            foreach (var id in ids)
            {
                foreach (char c in id)
                {
                    hash ^= c;
                    hash *= 16777619;
                }

                hash ^= '|';
                hash *= 16777619;
            }

            return hash;
        }
    }
}
