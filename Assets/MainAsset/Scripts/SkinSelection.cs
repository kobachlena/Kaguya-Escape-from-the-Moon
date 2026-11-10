using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SkinSelection : MonoBehaviour
{

    [Header("Swipe")]
    [Range(0.02f, 0.6f)] 
    [SerializeField] private float minSwipeScreenPercent = 0.08f;
    private Vector2 startTouchPosition;
    [SerializeField] private InputActionReference touchAction;
    [SerializeField] private InputActionReference pressAction;
    
    [Header("Skins")]
    [SerializeField] private GameObject[] skins;
    private int currentSkin;

    [Header("Animation")] 
    [SerializeField] private Vector3 rightPos;
    [SerializeField] private Vector3 leftPos;
    [SerializeField] private Vector3 endPos;
    [SerializeField] private float animSpeed;

    void Start()
    {
        currentSkin = 0;
        skins[0].transform.position = endPos;
        SelectSkin(0);
    }
    void Update()
    {
        ReadSwipe();
    }

    private void SelectSkin(int _index)
    {
        if((currentSkin == 0 && _index == -1) ||  (currentSkin == skins.Length - 1 &&   _index == 1))
            return;
        
        currentSkin = currentSkin + _index;
        
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
        
        Vector2 delta = currentTouchPosition - startTouchPosition;
        float minSwipeDistance = Screen.width * minSwipeScreenPercent;

        if (Mathf.Abs(delta.x) <= minSwipeDistance)
            return;

        if (delta.x > 0)
        {
            if (currentSkin == skins.Length - 1)
                return;
            SelectSkin(1);
            StartCoroutine(AnimateSkin(leftPos, endPos, animSpeed));
        }
        else
        {
            if(currentSkin == 0)
                return;
            SelectSkin(-1);
            StartCoroutine(AnimateSkin(rightPos, endPos, animSpeed));
        }

        startTouchPosition = currentTouchPosition;
    }
    
    // private void AnimateSkin(Vector3 startPosition, Vector3 endPosition, float animationSpeed)
    // {
    //     Transform skin = transform.GetChild(currentSkin);
    //     
    //     skin.localPosition = startPosition;
    //     skin.localPosition = Vector3.Lerp(skin.localPosition, endPosition, animationSpeed);
    // }


    IEnumerator AnimateSkin(Vector3 startPosition, Vector3 endPosition, float animationSpeed)
    {
        Transform skin = transform.GetChild(currentSkin);

        skin.localPosition = startPosition;
        while (Vector3.Distance(skin.localPosition, endPosition) > 0.0001f)
        {
            skin.localPosition = Vector3.Lerp(skin.localPosition, endPosition, animationSpeed);
            yield return null;
        }
        
        skin.localPosition = endPosition;
    }
}