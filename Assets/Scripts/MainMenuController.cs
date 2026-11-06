using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject logoImage;
    [SerializeField] private AudioClip mainMenuMusic;


    private void Start()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        logoImage.SetActive(true);
        AudioManager.Instance.PlayMusic(mainMenuMusic);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void SettingsOn()
    {
        mainPanel.SetActive(false);
        logoImage.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void SettingsOff()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        logoImage.SetActive(true);
    }

    public void StartGame()
    {
        AudioManager.Instance.StopMusic();
        SceneManager.LoadSceneAsync(1);
    }
    
    
    
    
}
