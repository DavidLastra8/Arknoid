namespace Arknoid.Core.Models
{
    /// <summary>Estado guardable de un ladrillo: qué ladrillo es y cuánta vida le queda.</summary>
    public readonly struct BrickState
    {
        public BrickState(string id, int hitPoints)
        {
            Id = id;
            HitPoints = hitPoints;
        }

        public string Id { get; }
        public int HitPoints { get; }
    }
}
