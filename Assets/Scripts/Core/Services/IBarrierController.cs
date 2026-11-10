namespace Arknoid.Core.Services
{
    public interface IBarrierController
    {
        /// <summary>Activa la barrera durante <paramref name="duration"/> segundos (si ya estaba activa, se renueva).</summary>
        void Activate(float duration);
    }
}
