using UnityEngine;

public class SkinValue : MonoBehaviour
{
    [Min(0)] [SerializeField] private int skinValue = 0;
    [SerializeField] bool isBought = false;

    public int GetSkinValue() { return skinValue;}
    public bool GetIsBought() { return isBought; }
}
