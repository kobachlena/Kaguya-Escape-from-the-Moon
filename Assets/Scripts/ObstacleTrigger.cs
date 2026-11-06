using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private GameManager gameManager;
    void OnTriggerEnter(Collider other)
    {
        if (gameManager == null)
        {
            Debug.LogError("ObstacleTrigger was not initialized.");
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            gameManager.GameOver();
        }
    }

    public void Initialize(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}
