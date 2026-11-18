using System;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Serializable]
    private struct SkinModels
    {
        public GameObject standing;
        public GameObject crouching;
    }

    [SerializeField] private SkinModels[] skins = Array.Empty<SkinModels>();

    private int selectedSkin;
    private bool isCrouching;
    private bool isVisible = true;

    internal void UseLegacyModels(GameObject[] standing, GameObject[] crouching)
    {
        if (skins.Length > 0 || standing == null || crouching == null)
            return;

        if (standing.Length != crouching.Length || standing.Length == 0)
        {
            Debug.LogError("Configure one standing and one crouching model per skin.", this);
            return;
        }

        skins = new SkinModels[standing.Length];
        for (int i = 0; i < skins.Length; i++)
        {
            skins[i].standing = standing[i];
            skins[i].crouching = crouching[i];
        }
    }

    public void SetSkin(int skinIndex)
    {
        selectedSkin = skinIndex >= 0 && skinIndex < skins.Length ? skinIndex : 0;
        RefreshModels();
    }

    public void SetCrouching(bool crouching)
    {
        if (isCrouching == crouching)
            return;

        isCrouching = crouching;
        RefreshModels();
    }

    public void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        RefreshModels();
    }

    private void RefreshModels()
    {
        for (int i = 0; i < skins.Length; i++)
        {
            bool showSkin = isVisible && selectedSkin == i;
            if (skins[i].standing != null)
                skins[i].standing.SetActive(showSkin && !isCrouching);
            if (skins[i].crouching != null)
                skins[i].crouching.SetActive(showSkin && isCrouching);
        }
    }
}