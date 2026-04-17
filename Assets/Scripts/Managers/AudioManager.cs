using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// Manager for all global audio and mixers
/// Persists across scenes
/// Holds and has calls for all global audio (Music, UI)
/// DOES NOT manage spatial audio (enemy, environment, player)
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    /// <summary>
    /// Singleton Manager setup (persistent)
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    [Header("Mixer and Sources")]
    public AudioMixer mixer;          // Your existing mixer with SFX/Music groups
    public AudioSource musicSource;   // For background music
    public AudioSource sfxSource;     // Optional: dedicated SFX source
    private float pauseVolume = -10f; // how much music volume decreases when game is paused 

    // ---------- Music ----------
    public void PlayMusic(AudioClip clip, bool loop = true)
    {   
        StopMusic(); //only one BGM track should play at a time
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        ChangeMusicVolume(-10f);
        Debug.Log("playing music");
    }

    public void ChangeMusicVolume(float volume)
    {   
        float p = pauseVolume;
        if (GameManager.Instance.State != GameManager.GameState.Paused)
        {
            p = 0f;
        }
        mixer.SetFloat("BGMVolume", volume + p);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Fades music out from the current volume to -40 dB over [duration] seconds
    /// </summary>
    /// <param name="duration"></param>
    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
        System.Collections.IEnumerator FadeOutCoroutine(float duration)
        {
            float current;
            mixer.GetFloat("BGMVolume", out current);
            float start = current;
            float end = -40f; // dB
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                ChangeMusicVolume(Mathf.Lerp(start, end, t / duration));
                yield return null;
            }

            StopMusic();
        }
    }

    

    // ---------- SFX ----------
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, volume);
    }

    // ---------- Volume Control ----------
    public void SetMasterVolume(float linear)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f);
    }

    public void SetBGMVolume(float linear)
    {
        mixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f);
    }

    public void SetSFXVolume(float linear)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f);
    }
}
