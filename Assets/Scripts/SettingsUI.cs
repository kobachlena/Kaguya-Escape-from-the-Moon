using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;


    private void Start()
    {
        LoadAudioSettings();
    }

    private void LoadAudioSettings()
    {
        float savedMasterVolume =
            PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);

        float savedMusicVolume =
            PlayerPrefs.GetFloat(AudioManager.MusicVolumeKey, 1f);

        float savedSfxVolume =
            PlayerPrefs.GetFloat(AudioManager.SfxVolumeKey, 1f);

        masterVolumeSlider.SetValueWithoutNotify(savedMasterVolume);
        musicVolumeSlider.SetValueWithoutNotify(savedMusicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(savedSfxVolume);

        SetMasterVolume(savedMasterVolume);
        SetMusicVolume(savedMusicVolume);
        SetSfxVolume(savedSfxVolume);
    }
    
    
    public void SetMasterVolume(float volume)
    {
        AudioManager.Instance.SetVolume(AudioManager.MasterVolumeParameter,  AudioManager.MasterVolumeKey, volume);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetVolume(AudioManager.MusicVolumeParameter, AudioManager.MusicVolumeKey, volume);
    }

    public void SetSfxVolume(float volume)
    {
        AudioManager.Instance.SetVolume(AudioManager.SfxVolumeParameter, AudioManager.SfxVolumeKey, volume);
    }
    
    
    public void SaveAudioSettings()
    {
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveAudioSettings();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveAudioSettings();
    }

    private void OnApplicationQuit()
    {
        SaveAudioSettings();
    }
}
