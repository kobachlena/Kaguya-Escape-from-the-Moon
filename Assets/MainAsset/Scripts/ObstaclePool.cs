using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [Min(1)] [SerializeField] private int obstaclesN = 3;
    [SerializeField] private GameManager gameManager;

    private readonly Queue<GameObject> available = new Queue<GameObject>();
    private readonly HashSet<GameObject> rented = new HashSet<GameObject>();
    private readonly List<GameObject> validPrefabs = new List<GameObject>();

    private void Awake()
    {
        if (obstaclePrefabs != null)
        {
            foreach (GameObject prefab in obstaclePrefabs)
            {
                if (prefab != null && prefab.GetComponent<ObstacleTrigger>() != null)
                    validPrefabs.Add(prefab);
                else
                    Debug.LogError("Obstacle prefab is missing or has no ObstacleTrigger on its root.", this);
            }
        }

        if (validPrefabs.Count == 0 || gameManager == null)
            Debug.LogError("ObstaclePool requires valid prefabs and a GameManager.", this);
    }

    public bool WarmUp(int capacity)
    {
        if (validPrefabs.Count == 0 || gameManager == null)
            return false;

        int target = Mathf.Max(obstaclesN, capacity);
        while (available.Count + rented.Count < target)
            CreateObstacle();

        return true;
    }

    private void CreateObstacle()
    {
        GameObject prefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
        GameObject obstacle = Instantiate(prefab, transform);
        obstacle.SetActive(false);
        obstacle.GetComponent<ObstacleTrigger>().Initialize(gameManager);
        available.Enqueue(obstacle);
    }

    public GameObject Get()
    {
        if (available.Count == 0)
            return null;

        GameObject obstacle = available.Dequeue();
        rented.Add(obstacle);
        return obstacle;
    }

    public void Return(GameObject obstacle)
    {
        if (obstacle == null || !rented.Remove(obstacle))
            return;

        obstacle.SetActive(false);
        obstacle.transform.SetParent(transform);
        available.Enqueue(obstacle);
    }
}
