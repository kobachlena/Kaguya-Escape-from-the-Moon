using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject logoImage;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private int gameSceneBuildIndex = 1;

    private bool isLoadingScene;


    private void Start()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        logoImage.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(mainMenuMusic);
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
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
        if (AudioManager.Instance != null)
            AudioManager.Instance.SaveSettings();

        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        logoImage.SetActive(true);
    }

    public void StartGame()
    {
        if (isLoadingScene)
            return;

        if (gameSceneBuildIndex < 0 || gameSceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("Game scene is not in Build Settings at the configured index.", this);
            return;
        }

        isLoadingScene = true;
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        SceneManager.LoadSceneAsync(gameSceneBuildIndex);
    }
    
    
    
    
}