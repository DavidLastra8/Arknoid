using Arknoid.Core.Services;

namespace Arknoid.Unity.Services
{
    public class UnityRandom : IRandom
    {
        // UnityEngine.Random.value devuelve [0, 1] inclusive; se recorta para respetar el contrato [0, 1).
        public float Value01() => UnityEngine.Random.value * 0.9999999f;

        public float Range(float min, float max) => UnityEngine.Random.Range(min, max);
    }
}
