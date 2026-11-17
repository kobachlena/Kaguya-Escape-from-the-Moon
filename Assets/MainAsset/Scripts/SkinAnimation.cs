using UnityEngine;

public class SkinAnimation : MonoBehaviour
{

    [SerializeField] private Transform rightPosition;
    [SerializeField] private Transform leftPosition;
    [SerializeField] private Transform centralPosition;
    [SerializeField] private float animationSpeed;

    private float swipeDirection;
    
    void OnEnable()
    {
        swipeDirection = GetComponentInParent<SkinSelection>().GetDeltaX();
        
        if(swipeDirection >= 0)
            transform.position = leftPosition.position;
        else
            transform.position = rightPosition.position;
    }
    
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, centralPosition.position, animationSpeed * Time.unscaledDeltaTime);
    }
}
