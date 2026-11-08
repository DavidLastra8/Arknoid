using Arknoid.Core.Services;
using UnityEngine;

namespace Arknoid.Unity.Services
{
    /// <summary>
    /// Reproduce los efectos de sonido. Sobrevive entre escenas (DontDestroyOnLoad).
    /// Ya no se accede con un "instance" estático: el SceneInstaller lo encuentra y lo inyecta como IAudioService.
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        private static AudioService persistent;

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip crashSound;
        [SerializeField] private AudioClip powerUpSound;

        private void Awake()
        {
            if (persistent != null && persistent != this)
            {
                // Copia duplicada en otra escena: se desactiva al momento para que el installer no la encuentre.
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            persistent = this;
            DontDestroyOnLoad(gameObject);
            if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            if (persistent == this) persistent = null;
        }

        public void Play(GameSound sound)
        {
            var clip = ClipFor(sound);
            if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
        }

        private AudioClip ClipFor(GameSound sound)
        {
            switch (sound)
            {
                case GameSound.Hit: return crashSound;
                case GameSound.LifeLost: return deathSound;
                case GameSound.PowerUp: return powerUpSound;
                default: return null;
            }
        }
    }
}
