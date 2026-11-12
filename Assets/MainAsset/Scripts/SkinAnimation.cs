using UnityEngine;

public class SkinAnimation : MonoBehaviour
{

    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    [SerializeField] private float animationSpeed;

    private float swipeDirection;
    
    void OnEnable()
    {
        swipeDirection = GetComponentInParent<SkinSelection>().GetDeltaX();
        
        if(swipeDirection >= 0)
            transform.position = startPosition;
        else
            transform.position = new Vector3(-startPosition.x, startPosition.y, startPosition.z);
    }
    
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, endPosition, animationSpeed * Time.deltaTime);
    }
}
