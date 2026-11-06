using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    
    [SerializeField] private GameObject[] obstaclePrefabs;
    [Min(1)]
    [SerializeField] private int obstaclesN = 3;
    [SerializeField] private GameManager gameManager;
    private Queue<GameObject> obstacles;

    void Awake()
    {
        obstacles = new Queue<GameObject>();
        
        for (int i = 0; i < obstaclesN; i++)
        {
            GameObject obstacle = Instantiate(obstaclePrefabs[Random.Range(0,obstaclePrefabs.Length)], transform);
            obstacle.GetComponent<ObstacleTrigger>().Initialize(gameManager);
            obstacle.SetActive(false);
            obstacles.Enqueue(obstacle);    
        }    
    }

    public void Return(GameObject obstacle)
    {
        obstacle.transform.SetParent(transform);
        obstacle.SetActive(false);
        obstacles.Enqueue(obstacle);
    }

    public GameObject Get()
    {
        if (obstacles.Count == 0) return null;
        GameObject obstacle = obstacles.Dequeue();
        obstacle.SetActive(true);
        return obstacle;
    }
    
    
    
}
