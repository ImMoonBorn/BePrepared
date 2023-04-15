using UnityEngine;
using MoonBorn.Utils;
using TMPro;
using MoonBorn.BePrepared.Utils.SaveSystem;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public enum ResourceType
    {
        None,
        Wood,
        Food,
        Stone
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        public static int WoodResources => Mathf.RoundToInt(Instance.m_WoodResources);
        public static int FoodResources => Mathf.RoundToInt(Instance.m_FoodResources);
        public static int StoneResources => Mathf.RoundToInt(Instance.m_StoneResources);

        public static int WoodAssignedCount => Instance.m_WoodAssignedCount;
        public static int FoodAssignedCount => Instance.m_FoodAssignedCount;
        public static int StoneAssignedCount => Instance.m_StoneAssignedCount;

        private float m_WoodResources = 0;
        private float m_FoodResources = 0;
        private float m_StoneResources = 0;

        private int m_WoodAssignedCount = 0;
        private int m_FoodAssignedCount = 0;
        private int m_StoneAssignedCount = 0;

        [Header("Icons")]
        [SerializeField] private Sprite m_WoodIcon;
        [SerializeField] private Sprite m_FoodIcon;
        [SerializeField] private Sprite m_StoneIcon;

        private void Start()
        {
            ResourceUI.OnChangeResources(Mathf.RoundToInt(m_WoodResources), Mathf.RoundToInt(m_FoodResources), Mathf.RoundToInt(m_StoneResources));
            ResourceUI.OnChangeAssinged(m_WoodAssignedCount, m_FoodAssignedCount, m_StoneAssignedCount);
        }

        public static void AddResource(ResourceType type, float amount)
        {
            Instance.ChangeResource(type, amount);
        }

        public static void SpendResource(ResourceType type, float amount)
        {
            Instance.ChangeResource(type, -amount);
        }

        public static bool CheckForResource(ResourceType type, int amount)
        {
            return type switch
            {
                ResourceType.Wood => WoodResources >= amount,
                ResourceType.Food => FoodResources >= amount,
                ResourceType.Stone => StoneResources >= amount,
                _ => false,
            };
        }

        private void ChangeResource(ResourceType type, float amount)
        {
            switch (type)
            {
                case ResourceType.Wood: m_WoodResources += amount; m_WoodResources = Mathf.Max(m_WoodResources, 0.0f); break;
                case ResourceType.Food: m_FoodResources += amount; m_FoodResources = Mathf.Max(m_FoodResources, 0.0f); break;
                case ResourceType.Stone: m_StoneResources += amount; m_StoneResources = Mathf.Max(m_StoneResources, 0.0f);  break;
            }

            ResourceUI.OnChangeResources(Mathf.RoundToInt(m_WoodResources), Mathf.RoundToInt(m_FoodResources), Mathf.RoundToInt(m_StoneResources));
        }

        public static Sprite GetIconByType(ResourceType type)
        {
            return type switch
            {
                ResourceType.Wood => Instance.m_WoodIcon,
                ResourceType.Food => Instance.m_FoodIcon,
                ResourceType.Stone => Instance.m_StoneIcon,
                _ => null,
            };
        }

        public static void AssignToResource(ResourceType type, int count = 1)
        {
            switch (type)
            {
                case ResourceType.Wood: Instance.m_WoodAssignedCount += count; break;
                case ResourceType.Food: Instance.m_FoodAssignedCount += count; break;
                case ResourceType.Stone: Instance.m_StoneAssignedCount += count; break;
            }

            ResourceUI.OnChangeAssinged(Instance.m_WoodAssignedCount, Instance.m_FoodAssignedCount, Instance.m_StoneAssignedCount);
        }

        public static void DeassignToResource(ResourceType type, int count = 1)
        {
            if (Instance == null)
                return;

            switch (type)
            {
                case ResourceType.Wood: Instance.m_WoodAssignedCount -= count; break;
                case ResourceType.Food: Instance.m_FoodAssignedCount -= count; break;
                case ResourceType.Stone: Instance.m_StoneAssignedCount -= count; break;
            }

            ResourceUI.OnChangeAssinged(Instance.m_WoodAssignedCount, Instance.m_FoodAssignedCount, Instance.m_StoneAssignedCount);
        }

        public static ResourceManagerData ResourceManagerData => new ResourceManagerData()
        { Wood = Instance.m_WoodResources, Food = Instance.m_FoodResources, Stone = Instance.m_StoneResources };

        public static void SetResourceManagerData(ResourceManagerData resourceManagerData)
        {
            Instance.m_WoodResources = resourceManagerData.Wood;
            Instance.m_FoodResources = resourceManagerData.Food;
            Instance.m_StoneResources = resourceManagerData.Stone;
            ResourceUI.OnChangeResources((int)resourceManagerData.Wood, (int)resourceManagerData.Food, (int)resourceManagerData.Stone);
        }


        private void Update()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    AddResource(ResourceType.Wood, 50);
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    AddResource(ResourceType.Food, 50);
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    AddResource(ResourceType.Stone, 50);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    SpendResource(ResourceType.Wood, 50);
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    SpendResource(ResourceType.Food, 50);
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    SpendResource(ResourceType.Stone, 50);
            }
        }
    }
}
