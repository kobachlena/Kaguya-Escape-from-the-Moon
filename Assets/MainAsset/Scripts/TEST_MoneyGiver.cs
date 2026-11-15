using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TEST_MoneyGiver : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [Header("Buttons")] 
    [SerializeField] private InputActionReference E;
    [SerializeField] private InputActionReference Q;
    [Min(1)]
    public int moneyValue = 0;
    
    public static TEST_MoneyGiver Instance{get; private set;}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this; 
    }

    private void Start()
    {
        moneyValue = SaveSystem.Instance.GetMoneyValue();
    }
    
    void Update()
    {
        if (E.action.WasPressedThisFrame())
        {
            moneyValue += 100;
            SaveSystem.Instance.SaveMoney(moneyValue);
        }

        if (Q.action.WasPressedThisFrame() && moneyValue >= 100)
        {
            moneyValue -= 100;
            SaveSystem.Instance.SaveMoney(moneyValue);
        }

        moneyText.text = "Money: "  + moneyValue.ToString() ;
    }
    
}
