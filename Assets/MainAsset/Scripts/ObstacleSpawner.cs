using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] obstacleSpawnPoints;
    [Min(0)] [SerializeField] private int obstaclesN = 3;

    private readonly List<Transform> validSpawnPoints = new List<Transform>();
    private readonly List<Transform> availableSpawnPoints = new List<Transform>();
    private readonly List<GameObject> spawnedObstacles = new List<GameObject>();
    private ObstaclePool obstaclePool;

    public int ObstacleCapacity => Mathf.Clamp(obstaclesN, 0, validSpawnPoints.Count);

    public void Initialize(ObstaclePool pool)
    {
        obstaclePool = pool;
        validSpawnPoints.Clear();
        if (obstacleSpawnPoints == null)
            return;

        foreach (Transform point in obstacleSpawnPoints)
        {
            if (point != null)
                validSpawnPoints.Add(point);
            else
                Debug.LogWarning("An obstacle spawn point is missing.", this);
        }
    }

    public void PlaceObstacles()
    {
        if (obstaclePool == null || spawnedObstacles.Count > 0)
            return;

        availableSpawnPoints.Clear();
        availableSpawnPoints.AddRange(validSpawnPoints);

        for (int i = 0; i < ObstacleCapacity; i++)
        {
            GameObject obstacle = obstaclePool.Get();
            if (obstacle == null)
            {
                Debug.LogWarning("Obstacle pool is empty.", this);
                break;
            }

            int index = Random.Range(0, availableSpawnPoints.Count);
            Transform point = availableSpawnPoints[index];
            availableSpawnPoints.RemoveAt(index);

            obstacle.transform.SetParent(transform);
            obstacle.transform.position = point.position;
            spawnedObstacles.Add(obstacle);
            obstacle.SetActive(true);
        }
    }

    public void ClearObstacles()
    {
        foreach (GameObject obstacle in spawnedObstacles)
            obstaclePool.Return(obstacle);

        spawnedObstacles.Clear();
    }
}
