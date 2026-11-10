using UnityEngine;

[RequireComponent(typeof(ObstacleSpawner))]
public class RoadSegment : MonoBehaviour
{
    private Transform player;
    private RoadSegmentManager roadSegmentManager;
    private ObstacleSpawner obstaclesSpawner;
    
   
    void Awake()
    {
        obstaclesSpawner = GetComponent<ObstacleSpawner>();
    }
    
    void Update()
    {
        if (player == null || roadSegmentManager == null || obstaclesSpawner == null) return;
        
        if (player.position.z > transform.position.z + roadSegmentManager.RoadLength * 1.5f)
        {
            obstaclesSpawner.ClearObstacles();
            roadSegmentManager.MoveRoadToEnd(gameObject);
            obstaclesSpawner.PlaceObstacles();
        }
    }

    public void Initialize(Transform player, RoadSegmentManager roadSegmentManager)
    {
        this.player = player;
        this.roadSegmentManager = roadSegmentManager;
    }

    public void Restart()
    {
        obstaclesSpawner.ClearObstacles();
        obstaclesSpawner.PlaceObstacles();
    }
    
}
