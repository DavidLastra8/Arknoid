namespace Arknoid.Core.Services
{
    public interface IRandom
    {
        /// <summary>Valor en [0, 1).</summary>
        float Value01();

        float Range(float min, float max);
    }
}
