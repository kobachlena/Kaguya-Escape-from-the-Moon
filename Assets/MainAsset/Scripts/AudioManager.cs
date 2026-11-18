using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public const string MasterVolumeKey = "MasterVolume";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SfxVolume";

    public const string MasterVolumeParameter = "MasterVolume";
    public const string MusicVolumeParameter = "MusicVolume";
    public const string SfxVolumeParameter = "SfxVolume";

    private const float MinimumVolumeDb = -80f;

    [Header("Audio Source")]
    [SerializeField] private AudioSource effectSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    public static AudioManager Instance { get; private set; }

    private bool settingsApplied;
    private bool settingsDirty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplySavedSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayEffect(AudioClip effectClip)
    {
        if (effectSource != null && effectClip != null)
            effectSource.PlayOneShot(effectClip);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null || musicClip == null)
            return;

        ApplySavedSettings();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public float GetVolume(string playerPrefsKey)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(playerPrefsKey, 1f));
    }

    public void SetVolume(string mixerParameter, string playerPrefsKey, float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (!ApplyVolume(mixerParameter, volume))
            return;

        PlayerPrefs.SetFloat(playerPrefsKey, volume);
        settingsDirty = true;
    }

    public void SaveSettings()
    {
        if (!settingsDirty)
            return;

        PlayerPrefs.Save();
        settingsDirty = false;
    }

    private void ApplySavedSettings()
    {
        if (settingsApplied)
            return;

        settingsApplied = true;
        ApplyVolume(MasterVolumeParameter, GetVolume(MasterVolumeKey));
        ApplyVolume(MusicVolumeParameter, GetVolume(MusicVolumeKey));
        ApplyVolume(SfxVolumeParameter, GetVolume(SfxVolumeKey));
    }

    private bool ApplyVolume(string mixerParameter, float volume)
    {
        float decibels = volume <= 0.0001f ? MinimumVolumeDb : Mathf.Log10(volume) * 20f;
        if (masterMixer != null && masterMixer.SetFloat(mixerParameter, decibels))
            return true;

        Debug.LogWarning("AudioMixer parameter is unavailable: " + mixerParameter, this);
        return false;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveSettings();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveSettings();
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}
