using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public const string MasterVolumeKey = "MasterVolume";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SfxVolume";

    public const string MasterVolumeParameter = "MasterVolume";
    public const string MusicVolumeParameter = "MusicVolume";
    public const string SfxVolumeParameter = "SfxVolume";

    private const float minimumVolumeDb = -80f;

    
    [Header("Audio Source")]
    [SerializeField] private AudioSource effectSource;
    [SerializeField] private AudioSource musicSource;
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;
    

    public static AudioManager Instance{get; private set;}

    void Awake()
    {
        if (Instance != null &&  Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    

    public void PlayEffect( AudioClip effectClip )
    {
        effectSource.PlayOneShot(effectClip);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    #region SetVolume

    
    public void SetVolume( string mixerParameter, string playerPrefsKey, float volume)
    {
        volume = Mathf.Clamp01(volume);

        float volumeInDecibels = ConvertToDecibels(volume);

        bool parameterFound = masterMixer.SetFloat(
            mixerParameter,
            volumeInDecibels
        );

        PlayerPrefs.SetFloat(playerPrefsKey, volume);
    }

    private float ConvertToDecibels(float volume)
    {
        if (volume <= 0.0001f)
            return minimumVolumeDb;

        return Mathf.Log10(volume) * 20f;
    }
    #endregion
    
    
}
