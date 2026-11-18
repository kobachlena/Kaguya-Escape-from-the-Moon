using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObstaclePool))]
public class RoadSegmentManager : MonoBehaviour, IRestartable
{
    [SerializeField] private Transform roadStartPoint;
    [SerializeField] private GameObject[] spawnedRoads;
    [Min(0.1f)] [SerializeField] private float roadLength = 220f;
    [SerializeField] private Transform player;

    public float RoadLength => roadLength;

    private readonly List<RoadSegment> segments = new List<RoadSegment>();
    private readonly Queue<RoadSegment> roadOrder = new Queue<RoadSegment>();
    private float lastSpawnedZ;
    private bool initialized;

    private void Start()
    {
        if (roadStartPoint == null || player == null || roadLength <= 0f ||
            spawnedRoads == null || spawnedRoads.Length < 2)
        {
            Debug.LogError("RoadSegmentManager requires a start point, a player and at least two road segments.", this);
            return;
        }

        ObstaclePool pool = GetComponent<ObstaclePool>();
        int totalCapacity = 0;
        foreach (GameObject road in spawnedRoads)
        {
            RoadSegment segment = road != null ? road.GetComponent<RoadSegment>() : null;
            if (segment == null || segments.Contains(segment))
            {
                Debug.LogError("Each Spawned Roads entry must be a different object with RoadSegment.", this);
                return;
            }

            segment.Initialize(pool);
            segments.Add(segment);
            totalCapacity += segment.ObstacleCapacity;
        }

        if (totalCapacity > 0 && !pool.WarmUp(totalCapacity))
            return;

        initialized = true;
        Restart();
    }

    private void Update()
    {
        if (!initialized || Time.timeScale == 0f)
            return;

        RoadSegment first = roadOrder.Peek();
        if (player.position.z <= first.transform.position.z + roadLength * 1.5f)
            return;

        roadOrder.Dequeue();
        first.ClearObstacles();

        lastSpawnedZ += roadLength;
        Vector3 position = first.transform.position;
        position.z = lastSpawnedZ;
        first.transform.position = position;

        first.PlaceObstacles();
        roadOrder.Enqueue(first);
    }

    public void Restart()
    {
        if (!initialized)
            return;

        foreach (RoadSegment segment in segments)
            segment.ClearObstacles();

        roadOrder.Clear();
        Vector3 startPosition = roadStartPoint.position;
        for (int i = 0; i < segments.Count; i++)
        {
            RoadSegment segment = segments[i];
            segment.transform.position = startPosition + Vector3.forward * (roadLength * i);
            segment.PlaceObstacles();
            roadOrder.Enqueue(segment);
        }

        lastSpawnedZ = startPosition.z + roadLength * (segments.Count - 1);
    }
}
