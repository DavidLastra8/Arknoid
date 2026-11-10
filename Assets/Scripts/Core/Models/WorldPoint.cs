namespace Arknoid.Core.Models
{
    /// <summary>Posición en el mundo sin depender de UnityEngine.Vector3 (Core no puede usar Unity).</summary>
    public readonly struct WorldPoint
    {
        public WorldPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }
}
