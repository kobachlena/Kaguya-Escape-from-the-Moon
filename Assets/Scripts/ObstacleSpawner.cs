using System.Collections.Generic;
using UnityEngine;


public class ObstacleSpawner : MonoBehaviour
{ 
    [SerializeField] Transform[] obstacleSpawnPoints;
    [SerializeField] private int obstaclesN = 3;
    private List<int> availableSpawnPoints;
    private ObstaclePool obstaclePool;
    private List<GameObject> spawnedObstacles;


    void Awake()
    {
        availableSpawnPoints = new List<int>();
        spawnedObstacles = new List<GameObject>();
    }

    public void PlaceObstacles()
    {
        availableSpawnPoints.Clear();
        
        for(int i = 0; i < obstacleSpawnPoints.Length; i++)
            availableSpawnPoints.Add(i);
        
        int obstaclesToSpawn  = Mathf.Min(obstaclesN, obstacleSpawnPoints.Length);
        
        for (int i = 0; i < obstaclesToSpawn ; i++)
        {
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            GameObject obstacle = obstaclePool.Get();
            int spawnPointIndex = availableSpawnPoints[randomIndex];
            Vector3 randomPosition =obstacleSpawnPoints[spawnPointIndex].position;
            if (obstacle == null)
            {
                Debug.Log("Obstacle pool is empty.");
                break;
            }
            obstacle.transform.SetParent(transform);
            obstacle.transform.position = randomPosition;
            
            spawnedObstacles.Add(obstacle);
            availableSpawnPoints.RemoveAt(randomIndex);
            
        }    
    }

    public void ClearObstacles()
    {
        foreach (GameObject obstacle in spawnedObstacles)
            obstaclePool.Return(obstacle);
        
        spawnedObstacles.Clear();
    }

    public void Initialize(ObstaclePool obstaclePool)
    {
        this.obstaclePool = obstaclePool;
        PlaceObstacles();
    }

}
