using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkinSelection : MonoBehaviour
{
    [Header("Swipe")]
    [Range(0.02f, 0.6f)] [SerializeField] private float minSwipeScreenPercent = 0.6f;
    [SerializeField] private InputActionReference touchAction;
    [SerializeField] private InputActionReference pressAction;

    [Header("Skins")]
    [SerializeField] private GameObject[] skins;
    [SerializeField] private int[] skinValue = { 0, 100, 1000 };
    [SerializeField] private Transform initialPosition;

    [Header("UI")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject buyButtons;
    [SerializeField] private TMP_Text moneyValueText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private GameManager gameManager;

    internal InputAction PressAction => pressAction != null ? pressAction.action : null;
    internal InputAction PositionAction => touchAction != null ? touchAction.action : null;

    private SaveSystem saveSystem;
    private int currentSkin;
    private float deltaX;
    private bool hasGesture;
    private bool initialized;
    private Vector2 swipeOrigin;

    private void Start()
    {
        saveSystem = SaveSystem.Instance;
        if (saveSystem == null || gameManager == null || skins == null || skins.Length == 0 ||
            skinValue == null || skinValue.Length != skins.Length ||
            playButton == null || buyButtons == null || PressAction == null || PositionAction == null)
        {
            Debug.LogError("SkinSelection requires matching models and prices, input actions, UI and SaveSystem.", this);
            enabled = false;
            return;
        }

        initialized = true;
        saveSystem.MoneyChanged += ShowBalance;
        RefreshOnOpen();
    }

    private void OnEnable()
    {
        hasGesture = false;
        if (initialized)
            RefreshOnOpen();
    }

    private void OnDestroy()
    {
        if (saveSystem != null)
            saveSystem.MoneyChanged -= ShowBalance;
    }

    private void Update()
    {
        if (!initialized || gameManager.CurrentState != GameState.Shopping)
        {
            hasGesture = false;
            return;
        }

        ReadSwipe();
    }

    private void RefreshOnOpen()
    {
        int selectedSkin = saveSystem.GetPlayableSkin();
        currentSkin = selectedSkin < skins.Length ? selectedSkin : 0;
        deltaX = 0f;
        ShowCurrentSkin();

        if (initialPosition != null && skins[currentSkin] != null)
            skins[currentSkin].transform.position = initialPosition.position;

        ShowBalance(saveSystem.GetMoneyValue());
    }

    public float GetDeltaX() => deltaX;

    private void ShowCurrentSkin()
    {
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] != null)
                skins[i].SetActive(i == currentSkin);
        }

        RefreshButtons();
    }

    private void ReadSwipe()
    {
        if (!PressAction.IsPressed())
        {
            hasGesture = false;
            return;
        }

        Vector2 position = PositionAction.ReadValue<Vector2>();
        if (PressAction.WasPressedThisFrame())
        {
            swipeOrigin = position;
            hasGesture = true;
            return;
        }

        if (!hasGesture)
            return;

        deltaX = position.x - swipeOrigin.x;
        if (Mathf.Abs(deltaX) <= Screen.width * minSwipeScreenPercent)
            return;

        int direction = deltaX > 0f ? 1 : -1;
        currentSkin = (currentSkin + direction + skins.Length) % skins.Length;
        swipeOrigin = position;
        ShowCurrentSkin();
    }

    private void RefreshButtons()
    {
        bool owned = saveSystem.IsSkinBought(currentSkin);
        playButton.SetActive(owned);
        buyButtons.SetActive(!owned);
        if (moneyValueText != null)
            moneyValueText.text = skinValue[currentSkin].ToString();
    }

    private void ShowBalance(int balance)
    {
        if (balanceText != null)
            balanceText.text = "Money: " + balance;
    }

    public void BuySkin()
    {
        if (!initialized || gameManager.CurrentState != GameState.Shopping)
            return;

        if (saveSystem.TryBuySkin(currentSkin, skinValue[currentSkin]))
            RefreshButtons();
    }

    public void Play()
    {
        if (!initialized || gameManager.CurrentState != GameState.Shopping)
            return;

        if (saveSystem.SetPlayableSkin(currentSkin))
            gameManager.ApplySelectedSkin();
    }
}
