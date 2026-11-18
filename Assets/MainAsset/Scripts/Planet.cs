using UnityEngine;

public class Planet : MonoBehaviour, IRestartable
{
    [SerializeField] private Vector3 startSize;
    [SerializeField] private Vector3 endSize;
    [Min(0.1f)] [SerializeField] private float rotationSpeed;
    [Min(0.1f)] [SerializeField] private float fullGrowthDistance = 2900f;
    [SerializeField] private PlayerMovement player;

    private void Update()
    {
        if (player == null || Time.timeScale == 0f)
            return;

        float progress = Mathf.Clamp01(player.GetDistance() / Mathf.Max(0.1f, fullGrowthDistance));
        transform.localScale = Vector3.Lerp(startSize, endSize, progress);
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    public void Restart()
    {
        transform.localScale = startSize;
    }
}