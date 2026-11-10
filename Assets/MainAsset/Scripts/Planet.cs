using System;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] private Vector3 startSize;
    [SerializeField] private Vector3 endSize;
    [Min(0.1f)]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private PlayerMovement player;
    private float progress;
    private float distanceP;
    
    void Update()
    {
       distanceP = player.GetDistance();
       IncreaseSize(distanceP);
       transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }


    void IncreaseSize(float distance)
    {
        progress = Mathf.Clamp01(distanceP / 2900);
        transform.localScale = Vector3.Lerp(startSize, endSize, progress);
        
    }
    
    public void Restart()
    {
        progress = 0f;
        distanceP = 0f;
        transform.localScale = startSize;
    }
}
