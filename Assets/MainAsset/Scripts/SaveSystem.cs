using System;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static readonly string BestScoreKey = "BestScore";
    public static readonly string MoneyValueKey = "MoneyValue";
    public static readonly string PlayableSkinKey = "PlayableSkin";
    private static readonly string[] BoughtSkinsKey = { "FirstSkin", "SecondSkin", "ThirdSkin" };

    public static SaveSystem Instance { get; private set; }
    public event Action<int> MoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SaveMoney(int moneyValue)
    {
        moneyValue = Mathf.Max(0, moneyValue);
        if (moneyValue == GetMoneyValue())
            return;

        PlayerPrefs.SetInt(MoneyValueKey, moneyValue);
        PlayerPrefs.Save();
        MoneyChanged?.Invoke(moneyValue);
    }

    public void SaveBestScore(int bestScore)
    {
        if (bestScore <= GetBestScore())
            return;

        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
    }

    public int GetBestScore() => PlayerPrefs.GetInt(BestScoreKey, 0);
    public int GetMoneyValue() => Mathf.Max(0, PlayerPrefs.GetInt(MoneyValueKey, 0));

    public bool IsSkinBought(int skinIndex)
    {
        if (!IsValidSkin(skinIndex))
            return false;

        return skinIndex == 0 || PlayerPrefs.GetInt(BoughtSkinsKey[skinIndex], 0) == 1;
    }

    public bool TryBuySkin(int skinIndex, int price)
    {
        if (!IsValidSkin(skinIndex) || IsSkinBought(skinIndex) || price < 0)
            return false;

        int balance = GetMoneyValue();
        if (balance < price)
            return false;

        int newBalance = balance - price;
        PlayerPrefs.SetInt(MoneyValueKey, newBalance);
        PlayerPrefs.SetInt(BoughtSkinsKey[skinIndex], 1);
        PlayerPrefs.Save();
        MoneyChanged?.Invoke(newBalance);
        return true;
    }

    public int GetPlayableSkin()
    {
        int skinIndex = PlayerPrefs.GetInt(PlayableSkinKey, 0);
        return IsSkinBought(skinIndex) ? skinIndex : 0;
    }

    public bool SetPlayableSkin(int skinIndex)
    {
        if (!IsSkinBought(skinIndex))
            return false;

        if (PlayerPrefs.GetInt(PlayableSkinKey, 0) != skinIndex)
        {
            PlayerPrefs.SetInt(PlayableSkinKey, skinIndex);
            PlayerPrefs.Save();
        }

        return true;
    }

    private static bool IsValidSkin(int skinIndex)
    {
        return skinIndex >= 0 && skinIndex < BoughtSkinsKey.Length;
    }
    
#if UNITY_EDITOR
    [ContextMenu("Debug/Reset Skin Purchase")]
    private void ResetSkinPurchase()
    {
        const int skinIndex = 2; 

        PlayerPrefs.DeleteKey(BoughtSkinsKey[skinIndex]);

        if (PlayerPrefs.GetInt(PlayableSkinKey, 0) == skinIndex)
            PlayerPrefs.SetInt(PlayableSkinKey, 0);

        PlayerPrefs.Save();
    }
#endif
}
