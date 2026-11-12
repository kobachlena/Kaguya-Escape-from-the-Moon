using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    
    public float GetDeltaX(){ return delta.x; }
    
    void Start()
    {
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
            transform.GetChild(i).gameObject.SetActive(i == currentSkin);
        }
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
}