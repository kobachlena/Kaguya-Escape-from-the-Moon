using TMPro;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine.InputSystem;
#endif

public class TEST_MoneyGiver : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Debug Buttons")]
    [SerializeField] private InputActionReference E;
    [SerializeField] private InputActionReference Q;
    private readonly HashSet<InputAction> enabledActions = new HashSet<InputAction>();
#endif

    private SaveSystem saveSystem;

    private void Start()
    {
        saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
            return;

        saveSystem.MoneyChanged += ShowMoney;
        ShowMoney(saveSystem.GetMoneyValue());
    }

    private void OnDestroy()
    {
        if (saveSystem != null)
            saveSystem.MoneyChanged -= ShowMoney;
    }

    private void ShowMoney(int balance)
    {
        if (moneyText != null)
            moneyText.text = "Money: " + balance;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnEnable()
    {
        EnableAction(E);
        EnableAction(Q);
    }

    private void OnDisable()
    {
        foreach (InputAction action in enabledActions)
            action.Disable();

        enabledActions.Clear();
    }

    private void Update()
    {
        if (saveSystem == null)
            return;

        int balance = saveSystem.GetMoneyValue();
        if (E != null && E.action != null && E.action.WasPressedThisFrame())
            saveSystem.SaveMoney(balance > int.MaxValue - 100 ? int.MaxValue : balance + 100);
        else if (Q != null && Q.action != null && Q.action.WasPressedThisFrame() && balance >= 100)
            saveSystem.SaveMoney(balance - 100);
    }

    private void EnableAction(InputActionReference reference)
    {
        InputAction action = reference != null ? reference.action : null;
        if (action == null || action.enabled)
            return;

        action.Enable();
        enabledActions.Add(action);
    }
#endif
}
