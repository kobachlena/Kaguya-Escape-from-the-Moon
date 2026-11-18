using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void OnEnable()
    {
        RefreshSliders();
    }

    private void Start()
    {
        RefreshSliders();
    }

    private void RefreshSliders()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio == null)
            return;

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(audio.GetVolume(AudioManager.MasterVolumeKey));
        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(audio.GetVolume(AudioManager.MusicVolumeKey));
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(audio.GetVolume(AudioManager.SfxVolumeKey));
    }

    public void SetMasterVolume(float volume)
    {
        AudioManager.Instance?.SetVolume(AudioManager.MasterVolumeParameter, AudioManager.MasterVolumeKey, volume);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance?.SetVolume(AudioManager.MusicVolumeParameter, AudioManager.MusicVolumeKey, volume);
    }

    public void SetSfxVolume(float volume)
    {
        AudioManager.Instance?.SetVolume(AudioManager.SfxVolumeParameter, AudioManager.SfxVolumeKey, volume);
    }

    public void SaveAudioSettings()
    {
        AudioManager.Instance?.SaveSettings();
    }
}