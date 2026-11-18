using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    AwaitingStart = 0,
    Playing = 1,
    Paused = 2,
    GameOver = 3,
    Shopping = 4,
    Dying = 5
}

public class GameManager : MonoBehaviour, IRestartable
{
    public GameState CurrentState { get; private set; } = GameState.AwaitingStart;

    [Header("UI")]
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private TMP_Text restartText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject tapPanel;
    [SerializeField] private GameObject shopButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private SkinSelection skinSelection;

    [Header("Run")]
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Planet planet;
    [SerializeField] private RoadSegmentManager roadSegmentManager;
    [SerializeField] private Score score;
    [SerializeField] private InputActionReference touchAction;

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip uiClip;

    private readonly HashSet<InputAction> enabledActions = new HashSet<InputAction>();
    private readonly PointerUiHitTest uiHitTest = new PointerUiHitTest();
    private Coroutine deathRoutine;
    private int stateEnteredFrame;
    private bool initialized;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (skinSelection == null && shopPanel != null)
            skinSelection = shopPanel.GetComponentInChildren<SkinSelection>(true);

        if (player == null || planet == null || roadSegmentManager == null ||
            score == null || restartPanel == null || restartText == null ||
            pausePanel == null || pauseButton == null || scorePanel == null ||
            tapPanel == null || touchAction == null || touchAction.action == null)
        {
            Debug.LogError("GameManager has missing run or UI references.", this);
            enabled = false;
            return;
        }

        if (explosionEffect != null)
        {
            ParticleSystem.MainModule main = explosionEffect.main;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.None;
            explosionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        initialized = true;
        EnterState(GameState.AwaitingStart);
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        EnableAction(touchAction.action);
        EnableAction(player.PressAction);
        EnableAction(player.PositionAction);

        if (skinSelection != null)
        {
            EnableAction(skinSelection.PressAction);
            EnableAction(skinSelection.PositionAction);
        }

        if (CurrentState == GameState.Dying)
            ShowGameOver();
        else if (CurrentState == GameState.Playing)
            EnterState(GameState.AwaitingStart);
    }

    private void Start()
    {
        ApplySelectedSkin();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(gameMusic);
    }

    private void OnDisable()
    {
        foreach (InputAction action in enabledActions)
            action.Disable();

        enabledActions.Clear();

        if (deathRoutine != null)
        {
            StopCoroutine(deathRoutine);
            deathRoutine = null;
        }

        if (initialized && player != null)
            player.enabled = false;
    }

    private void OnDestroy()
    {
        if (initialized)
            Time.timeScale = 1f;
    }

    private void LateUpdate()
    {
        if (CurrentState != GameState.AwaitingStart || Time.frameCount == stateEnteredFrame ||
            !touchAction.action.WasPressedThisFrame())
            return;

        if (PointerUiHitTest.TryGetPosition(touchAction.action, out Vector2 position) &&
            uiHitTest.IsOverUi(position))
            return;

        EnterState(GameState.Playing);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized && CurrentState == GameState.Playing)
            Pause();
    }

    public void Pause()
    {
        if (initialized && (CurrentState == GameState.Playing || CurrentState == GameState.AwaitingStart))
            EnterState(GameState.Paused);
    }

    public void Resume()
    {
        if (initialized && (CurrentState == GameState.Paused || CurrentState == GameState.Shopping))
            EnterState(GameState.AwaitingStart);
    }

    public void Shop()
    {
        if (initialized && shopPanel != null &&
            (CurrentState == GameState.Playing || CurrentState == GameState.AwaitingStart))
            EnterState(GameState.Shopping);
    }

    public void GameOver()
    {
        if (CurrentState != GameState.Playing)
            return;

        EnterState(GameState.Dying);
        player.GameOver();

        int distance = player.GetDistance();
        score.CheckBestScore();
        restartText.text = "Score:\n" + distance;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEffect(explosionClip);
            AudioManager.Instance.StopMusic();
        }

        if (explosionEffect == null)
        {
            ShowGameOver();
            return;
        }

        explosionEffect.transform.position = player.transform.position;
        explosionEffect.gameObject.SetActive(true);
        explosionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosionEffect.Play(true);
        deathRoutine = StartCoroutine(WaitForExplosion());
    }

    public void Restart()
    {
        if (CurrentState != GameState.GameOver)
            return;

        if (explosionEffect != null)
            explosionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        player.Restart();
        planet.Restart();
        roadSegmentManager.Restart();
        EnterState(GameState.AwaitingStart);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(gameMusic);
    }

    public void ApplySelectedSkin()
    {
        if (player != null)
            player.Play();
    }

    public void Quit()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnterState(GameState state)
    {
        CurrentState = state;
        stateEnteredFrame = Time.frameCount;

        bool playing = state == GameState.Playing;
        bool waiting = state == GameState.AwaitingStart;
        bool shopping = state == GameState.Shopping;

        Time.timeScale = playing || state == GameState.Dying ? 1f : 0f;
        player.enabled = playing;

        tapPanel.SetActive(waiting);
        scorePanel.SetActive(playing);
        pausePanel.SetActive(state == GameState.Paused);
        restartPanel.SetActive(state == GameState.GameOver);
        pauseButton.SetActive(waiting || playing);

        if (shopButton != null)
            shopButton.SetActive((waiting || playing) && shopPanel != null);

        if (shopPanel != null)
            shopPanel.SetActive(shopping);
    }

    private void EnableAction(InputAction action)
    {
        if (action == null || action.enabled)
            return;

        action.Enable();
        enabledActions.Add(action);
    }

    private IEnumerator WaitForExplosion()
    {
        yield return new WaitForSecondsRealtime(3f);
        deathRoutine = null;
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        EnterState(GameState.GameOver);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEffect(uiClip);
    }
}
