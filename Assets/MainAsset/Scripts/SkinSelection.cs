using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class SkinSelection : MonoBehaviour
{

    [Header("Swipe")]
    [Range(0.02f, 0.6f)] 
    [SerializeField] private float minSwipeScreenPercent = 0.6f;
    private Vector2 startTouchPosition;
    [SerializeField] private InputActionReference touchAction;
    [SerializeField] private InputActionReference pressAction;
    
    [Header("Skins")]
    [SerializeField] private GameObject[] skins;
    private int currentSkin;

    [Header("Animation")] 
    [SerializeField] private Vector3 initialPosition;
    private Vector2 delta = Vector2.zero;
    
    [Header("Buttons")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject buyButtons;
    
    [Header("Optional")]
    [SerializeField] TEST_MoneyGiver moneyGiver;
    [SerializeField] TMP_Text moneyValueText;
    
    [Header("SkinAttributes")]
    [SerializeField] private int[] skinValue = new int[3]{ 0, 100, 1000};
    [SerializeField] private bool[] isBought = new bool[3]{ true, false, false};
    
    
    public float GetDeltaX(){ return delta.x; }
    
    void Start()
    {
        for (int i = 0; i < isBought.Length; i++)
        {
            isBought[i] = SaveSystem.Instance.isSkinBought(i);
        }
        currentSkin = 0;
        skins[0].SetActive(true);
        skins[0].transform.position = initialPosition;
        SelectSkin();
    }
    
    void Update()
    {
        ReadSwipe();
    }
    
    private void SelectSkin()
    {
        for (int i = 0; i < skins.Length; i++)
        {
            skins[i].SetActive(i == currentSkin);
        }

        ButtonSwap();
    }

    private void ReadSwipe()
    {
        Vector2 currentTouchPosition = touchAction.action.ReadValue<Vector2>();
        
        if (pressAction.action.WasPressedThisFrame())
        {
            startTouchPosition = touchAction.action.ReadValue<Vector2>();
            return;
        }

        if (!pressAction.action.IsPressed())
            return;
        
        delta = currentTouchPosition - startTouchPosition;
        float minSwipeDistance = Screen.width * minSwipeScreenPercent;

        if (Mathf.Abs(delta.x) <= minSwipeDistance)
            return;

        if (delta.x > 0)
        {
            if (currentSkin == skins.Length - 1)
                currentSkin = 0;
            else
                currentSkin++;
            
            SelectSkin();
        }
        
        else
        {
            if(currentSkin == 0)
                currentSkin = skins.Length - 1;
            else
                currentSkin--;
            
            SelectSkin();
            
        }

        startTouchPosition = currentTouchPosition;
    }

    
    
    private void ButtonSwap()
    {
        if (isBought[currentSkin])
        {
            playButton.SetActive(true);
            buyButtons.SetActive(false);
        }
        else if (!isBought[currentSkin])
        {
            playButton.SetActive(false);
            buyButtons.SetActive(true);
            moneyValueText.text = skinValue[currentSkin].ToString();     
        }

    }

    public void BuySkin()
    {
        if (isBought[currentSkin])
            return;
        if (TEST_MoneyGiver.Instance.moneyValue < skinValue[currentSkin])
            return;
            
        isBought[currentSkin] = true;
        moneyGiver.moneyValue -= skinValue[currentSkin];
            
        SaveSystem.Instance.SaveBoughtSkin(currentSkin);
        SaveSystem.Instance.SaveMoney(TEST_MoneyGiver.Instance.moneyValue);
        
        ButtonSwap();

    }
    
}