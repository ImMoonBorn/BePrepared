using UnityEngine;
using MoonBorn.Utils;
using TMPro;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public enum ResourceType
    {
        Wood,
        Food
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        public static int WoodResources => Instance.m_WoodResources;
        public static int FoodResources => Instance.m_FoodResources;

        public static int WoodAssignedCount => Instance.m_WoodAssignedCount;
        public static int FoodAssignedCount => Instance.m_FoodAssignedCount;

        private int m_WoodResources = 0;
        private int m_FoodResources = 0;

        private int m_WoodAssignedCount = 0;
        private int m_FoodAssignedCount = 0;

        [Header("Icons")]
        [SerializeField] private Sprite m_WoodIcon;
        [SerializeField] private Sprite m_FoodIcon;

        private void Start()
        {
            ResourceUI.OnChangeResources(m_WoodResources, m_FoodResources);
            ResourceUI.OnChangeAssinged(m_WoodAssignedCount, m_FoodAssignedCount);

        }

        public static void AddResource(ResourceType type, int amount)
        {
            Instance.ChangeResource(type, amount);
        }

        public static void SpendResource(ResourceType type, int amount)
        {
            Instance.ChangeResource(type, -amount);
        }

        public static bool CheckForResource(ResourceType type, int amount)
        {
            return type switch
            {
                ResourceType.Wood => WoodResources >= amount,
                ResourceType.Food => FoodResources >= amount,
                _ => false,
            };
        }

        private void ChangeResource(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Wood: m_WoodResources += amount; break;
                case ResourceType.Food: m_FoodResources += amount; break;
            }

            ResourceUI.OnChangeResources(m_WoodResources, m_FoodResources);
        }

        public static Sprite GetIconByType(ResourceType type)
        {
            return type switch
            {
                ResourceType.Wood => Instance.m_WoodIcon,
                ResourceType.Food => Instance.m_FoodIcon,
                _ => null,
            };
        }

        public static void AssignToResource(ResourceType type, int count = 1)
        {
            switch (type)
            {
                case ResourceType.Wood: Instance.m_WoodAssignedCount += count; break;
                case ResourceType.Food: Instance.m_FoodAssignedCount += count; break;
            }

            ResourceUI.OnChangeAssinged(Instance.m_WoodAssignedCount, Instance.m_FoodAssignedCount);
        }

        public static void DeassignToResource(ResourceType type, int count = 1)
        {
            if (Instance == null)
                return;

            switch (type)
            {
                case ResourceType.Wood: Instance.m_WoodAssignedCount -= count; break;
                case ResourceType.Food: Instance.m_FoodAssignedCount -= count; break;
            }

            ResourceUI.OnChangeAssinged(Instance.m_WoodAssignedCount, Instance.m_FoodAssignedCount);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddResource(ResourceType.Wood, 50);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddResource(ResourceType.Food, 50);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SpendResource(ResourceType.Wood, 50);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SpendResource(ResourceType.Food, 50);
            }
        }
    }
}
