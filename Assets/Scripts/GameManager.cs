using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GameState
{
    Countdown,
    Playing,
    Paused,
    GameOver
}

public interface IRestartable
{
    void Restart(); 
}

public class GameManager : MonoBehaviour, IRestartable
{
    public GameState CurrentState{get; private set; }
    
    [Header("UI")]
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private TMP_Text restartText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TMP_Text countDownText;
    [SerializeField] GameObject countDown;
    [SerializeField] private GameObject tapPanel;
    
    [SerializeField] private int countDownValue = 3;
    private Coroutine countdownCoroutine;
    private Coroutine waitForTapCoroutine;
    
    [Header("TechnicalThings")]
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Planet planet;
    [SerializeField] private RoadSegmentManager roadSegmentManager;
    [SerializeField] private Score score;
    [SerializeField] private InputActionReference touchAction;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem explosionEffect;
    
    [Header("Sounds")]
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField]  private AudioClip uiClip;
    
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        CurrentState = GameState.Playing;
        if (restartPanel == null)
        {
            Debug.LogError("Game over panel is not assigned.");
            return;
        }
        
        restartPanel.SetActive(false);
        pausePanel.SetActive(false);
        countDown.SetActive(false);
        scorePanel.SetActive(false);
        pauseButton.SetActive(false);
        tapPanel.SetActive(true);
    }

    void Start()
    {
        TapToContinue();
    }
    
    void OnEnable(){touchAction.action.Enable();}
    void OnDisable(){touchAction.action.Disable();}

    IEnumerator CountDown()
    {
        pausePanel.SetActive(false);
        countDown.SetActive(true);
        int countDownv = countDownValue;
        
        while (countDownv >= 0)
        {
            if(countDownv==0)
                countDownText.text = "GO!";
            else
             countDownText.text = countDownv.ToString();
            yield return new WaitForSecondsRealtime(0.7f);
            countDownv--;
        }
        countDown.SetActive(false);
        scorePanel.SetActive(true);
        pauseButton.SetActive(true);
        player.enabled = true;
        Time.timeScale = 1f;
        CurrentState = GameState.Playing;
        countdownCoroutine = null;
    }
    
    
    public void Pause()
    {
        if (CurrentState != GameState.Playing)
            return;
        Time.timeScale = 0f;
        player.enabled = false;
        CurrentState = GameState.Paused;
        scorePanel.SetActive(false);
        pauseButton.SetActive(false);
        AudioManager.Instance.PlayEffect(uiClip);
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (CurrentState != GameState.Paused)
            return;
        BeginCountdown();
    }

   
    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;
        player.GameOver();
        player.enabled = false;
        CurrentState = GameState.GameOver;
        score.CheckBestScore();
        restartText.text = "Score:\n" + player.GetDistance().ToString();
        scorePanel.SetActive(false);
        pauseButton.SetActive(false);
        AudioManager.Instance.PlayEffect(explosionClip);
        AudioManager.Instance.StopMusic();
        StartCoroutine(SetExplosion());
    }

    public void Restart()
    {
        if (CurrentState != GameState.GameOver)
            return;

        player.Restart();
        planet.Restart();
        roadSegmentManager.Restart();

        restartPanel.SetActive(false);

        TapToContinue();
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void BeginCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        
        Time.timeScale = 0f;
        player.enabled = false;
        CurrentState = GameState.Countdown;

        countdownCoroutine = StartCoroutine(CountDown());
    }

    IEnumerator SetExplosion()
    {
        explosionEffect.gameObject.SetActive(true);
        explosionEffect.transform.position = player.transform.position;
        explosionEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
        explosionEffect.Play(true);
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 0f;
        AudioManager.Instance.PlayEffect(uiClip);
        restartPanel.SetActive(true);
    }


    private void TapToContinue()
    {
        scorePanel.SetActive(false);
        pauseButton.SetActive(false);
        tapPanel.SetActive(true);
        Time.timeScale = 0f;
        player.enabled = false;
        touchAction.action.Enable();
        CurrentState = GameState.Countdown;
        if (waitForTapCoroutine != null)
            StopCoroutine(waitForTapCoroutine);
        
        waitForTapCoroutine = StartCoroutine(WaitForTap());

    }

    private IEnumerator WaitForTap()
    {
        yield return null;
        
        AudioManager.Instance.PlayMusic(gameMusic);
        
        while (!touchAction.action.WasPressedThisFrame())
        {
            yield return null;
        }

        waitForTapCoroutine = null;
        tapPanel.SetActive(false);
        scorePanel.SetActive(true);
        pauseButton.SetActive(true);
        Time.timeScale = 1f;
        player.enabled = true;
        CurrentState = GameState.Playing;
    }
}
