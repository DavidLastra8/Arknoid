using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource sfxSource;
    public AudioClip deathSound;
    public AudioClip crashSound;
    public AudioClip powerUpSound;

    public static SoundManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }


    }
    public void PlaySound(AudioClip clip)
    {
        instance.sfxSource.clip = clip;
        instance.sfxSource.Play();
    }
}