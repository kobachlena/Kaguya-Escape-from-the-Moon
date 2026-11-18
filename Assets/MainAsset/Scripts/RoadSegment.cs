using UnityEngine;

[RequireComponent(typeof(ObstacleSpawner))]
public class RoadSegment : MonoBehaviour
{
    private ObstacleSpawner spawner;

    public int ObstacleCapacity => spawner.ObstacleCapacity;

    public void Initialize(ObstaclePool pool)
    {
        spawner = GetComponent<ObstacleSpawner>();
        spawner.Initialize(pool);
    }

    public void PlaceObstacles()
    {
        spawner.PlaceObstacles();
    }

    public void ClearObstacles()
    {
        spawner.ClearObstacles();
    }

    public void Restart()
    {
        ClearObstacles();
        PlaceObstacles();
    }
}