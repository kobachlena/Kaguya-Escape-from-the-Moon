using UnityEngine;

[DisallowMultipleComponent]

public class SkinAnimation : MonoBehaviour
{

    [SerializeField] private Transform rightPosition;
    [SerializeField] private Transform leftPosition;
    [SerializeField] private Transform centralPosition;
    [SerializeField] private float animationSpeed;

    private SkinSelection skinSelection;

    private void Awake()
    {
        skinSelection = GetComponentInParent<SkinSelection>();
    }
    
    void OnEnable()
    {
        if (skinSelection == null || leftPosition == null || rightPosition == null)
            return;

        float swipeDirection = skinSelection.GetDeltaX();
        
        if(swipeDirection >= 0)
            transform.position = leftPosition.position;
        else
            transform.position = rightPosition.position;
    }
    
    void Update()
    {
        if (centralPosition != null)
            transform.position = Vector3.Lerp(transform.position, centralPosition.position, animationSpeed * Time.unscaledDeltaTime);
    }
}