using UnityEngine;
using TMPro;
using MoonBorn.Utils;

public class ResourceUI : Singleton<ResourceUI>
{
    [SerializeField] private TMP_Text m_WoodResourcesText;
    [SerializeField] private TMP_Text m_FoodResourcesText;
    [SerializeField] private TMP_Text m_StoneResourcesText;

    [Header("Texts")]
    [SerializeField] private TMP_Text m_WoodAssignedText;
    [SerializeField] private TMP_Text m_FoodAssignedText;
    [SerializeField] private TMP_Text m_StoneAssignedText;

    public static void OnChangeResources(int woodCount, int foodCount, int stoneCount)
    {
        Instance.m_WoodResourcesText.text = woodCount.ToString();
        Instance.m_FoodResourcesText.text = foodCount.ToString();
        Instance.m_StoneResourcesText.text = stoneCount.ToString();
    }

    public static void OnChangeAssinged(int woodCount, int foodCount, int stoneCount)
    {
        Instance.m_WoodAssignedText.text = woodCount.ToString();
        Instance.m_FoodAssignedText.text = foodCount.ToString();
        Instance.m_StoneAssignedText.text = stoneCount.ToString();
    }
}
