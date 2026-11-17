using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static readonly string BestScoreKey = "BestScore";
    public static readonly string MoneyValueKey = "MoneyValue";
    public static readonly string[] BoughtSkinsKey = { "FirstSkin", "SecondSkin", "ThirdSkin" };
    public static readonly string PlayableSkinKey = "PlayableSkin";
    public static SaveSystem Instance{get; private set;}
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this; 
        DontDestroyOnLoad(gameObject);

        SaveBoughtSkin(0);
    }


    public void SaveMoney(int moneyValue)
    {
        PlayerPrefs.SetInt(MoneyValueKey, moneyValue);
        PlayerPrefs.Save();
    }

    public void SaveBestScore(int  bestScore)
    {
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
    }

    public void SaveBoughtSkin(int skinIndex)
    {
        PlayerPrefs.SetInt(BoughtSkinsKey[skinIndex], 1);
        PlayerPrefs.Save();
    }
    
    public int GetBestScore(){return PlayerPrefs.GetInt(BestScoreKey, 0);}
    
    public int GetMoneyValue(){return PlayerPrefs.GetInt(MoneyValueKey, 0);}

    public bool isSkinBought(int skinIndex)
    {
        return PlayerPrefs.GetInt(BoughtSkinsKey[skinIndex], 0) == 1;
    }

    public int GetPlayableSkin()
    {
        return PlayerPrefs.GetInt(PlayableSkinKey, 0);
    }

    public void SetPlayableSkin(int skinIndex)
    {
        PlayerPrefs.SetInt(PlayableSkinKey, skinIndex);
        PlayerPrefs.Save();
    }
}
