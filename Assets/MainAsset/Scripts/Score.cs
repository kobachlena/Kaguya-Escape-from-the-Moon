using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private PlayerMovement player;
    private int bestScoreValue;
    private int displayedScore = -1;

    void Start()
    {
        bestScoreValue = SaveSystem.Instance.GetBestScore();
        bestScoreText.text = "Best Score:\n" + bestScoreValue.ToString();
        displayedScore = -1;
    }
    
    void Update()
    {
        int currentScore = player.GetDistance();
        if (currentScore == displayedScore)
            return;
        
        displayedScore = currentScore;
        scoreText.text = "Score:\n" + currentScore.ToString();
    }

    public void CheckBestScore()
    {
        int currentScore = player.GetDistance();

        if (currentScore <= bestScoreValue)
            return;

        bestScoreValue = currentScore;

        SaveSystem.Instance.SaveBestScore(bestScoreValue);

        bestScoreText.text =
            "Best Score:\n" + bestScoreValue;
    }
}
