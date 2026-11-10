using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public const string BestScoreKey = "BestScore";
    
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private PlayerMovement player;
    private int bestScoreValue;
    private int displayedScore = -1;

    void Start()
    {
        bestScoreValue = PlayerPrefs.GetInt(BestScoreKey, 0);
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

        PlayerPrefs.SetInt(BestScoreKey, bestScoreValue);
        PlayerPrefs.Save();

        bestScoreText.text =
            "Best Score:\n" + bestScoreValue;
    }
}
