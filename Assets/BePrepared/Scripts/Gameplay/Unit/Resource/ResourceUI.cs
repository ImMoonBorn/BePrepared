using UnityEngine;
using MoonBorn.Utils;
using TMPro;

public class ResourceUI : Singleton<ResourceUI>
{
    [SerializeField] private TMP_Text m_WoodResourcesText;
    [SerializeField] private TMP_Text m_FoodResourcesText;

    [Header("Texts")]
    [SerializeField] private TMP_Text m_WoodAssignedText;
    [SerializeField] private TMP_Text m_FoodAssignedText;

    public static void OnChangeResources(int woodCount, int foodCount)
    {
        Instance.m_WoodResourcesText.text = woodCount.ToString();
        Instance.m_FoodResourcesText.text = foodCount.ToString();
    }

    public static void OnChangeAssinged(int woodCount, int foodCount)
    {
        Instance.m_WoodAssignedText.text = woodCount.ToString();
        Instance.m_FoodAssignedText.text = foodCount.ToString();
    }
}
