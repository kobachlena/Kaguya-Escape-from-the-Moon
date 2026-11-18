using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (gameManager == null)
        {
            Debug.LogError("ObstacleTrigger was not initialized.", this);
            return;
        }

        gameManager.GameOver();
    }

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }
}