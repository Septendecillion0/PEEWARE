using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer and Sources")]
    public AudioMixer mixer;          // Your existing mixer with SFX/Music groups
    public AudioSource musicSource;   // For background music
    public AudioSource sfxSource;     // Optional: dedicated SFX source
    private float pauseVolume = -10f;

    // ---------- Music ----------
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        ChangeMusicVolume(-10f);
        Debug.Log("playing music");
    }

    public void ChangeMusicVolume(float volume)
    {   
        float p = pauseVolume;
        if (!GameManager.Instance.IsPaused)
        {
            p = 0f;
        }
        mixer.SetFloat("BGMVolume", volume + p);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
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
