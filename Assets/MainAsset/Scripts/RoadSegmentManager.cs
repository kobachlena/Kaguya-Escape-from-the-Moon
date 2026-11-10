using UnityEngine;

[RequireComponent(typeof(ObstaclePool))]
public class RoadSegmentManager : MonoBehaviour, IRestartable
{
    //[SerializeField] private GameObject roadPrefab;
    [SerializeField] private Transform roadStartPoint;

    [SerializeField] private GameObject[] spawnedRoads;
    [Min(0.1f)]
    [SerializeField] private float roadLength = 220f;
    public float RoadLength => roadLength;
    private float lastSpawnedZ;
    
    [SerializeField] private Transform player;
    private ObstaclePool obstaclePool;
    void Awake()
    {
        obstaclePool = GetComponent<ObstaclePool>();
    }
    
    void Start() 
    {
        
        // if (roadPrefab == null)
        // {
        //     Debug.LogError("Road prefab is not assigned.");
        //     return;
        // }

        if (roadStartPoint == null)
        {
            Debug.LogError("Road start point is not assigned.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is not assigned.");
            return;
        }
        
        for (int i = 0; i < spawnedRoads.Length; i++)//спавн дороги
        {
            GameObject road = spawnedRoads[i];
            road.GetComponent<RoadSegment>().Initialize(player, this);
            road.GetComponent<ObstacleSpawner>().Initialize(obstaclePool);
            spawnedRoads[i] = road;
        }
        
        lastSpawnedZ = roadStartPoint.position.z +  roadLength *  (spawnedRoads.Length - 1);
    }


    public void MoveRoadToEnd(GameObject road)
    {
        lastSpawnedZ += roadLength;
        road.transform.position = new Vector3(road.transform.position.x, road.transform.position.y, lastSpawnedZ);
    }

    public void Restart()
    {
        for (int i = 0; i < spawnedRoads.Length; i++)
        {
            spawnedRoads[i].transform.position = new Vector3( roadStartPoint.position.x, roadStartPoint.position.y, roadStartPoint.position.z + roadLength * i);
            spawnedRoads[i].GetComponent<RoadSegment>().Restart();
        }
        lastSpawnedZ = roadStartPoint.position.z + roadLength * (spawnedRoads.Length - 1);;
    }
}
