using System.Collections.Generic;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitResource : MonoBehaviour
    {
        public ResourceType ResourceType => m_ResourceSO.ResourceType;
        public int ResourceAmount => m_ResourceAmount;
        public int ResourceAmountMax => m_ResourceAmountMax;

        [SerializeField] private UnitResourceSO m_ResourceSO;
        [SerializeField] private int m_ResourceAmountMax = 100;
        private int m_ResourceAmount = 0;
        private bool m_Destroyed = false;
        private List<UnitVillager> m_VillagerList = new();

        private void Awake()
        {
            m_ResourceAmount = m_ResourceAmountMax;
        }

        public void Gather(int amount)
        {
            if (m_Destroyed)
                return;

            if (amount > m_ResourceAmount)
            {
                ResourceManager.AddResource(ResourceType, m_ResourceAmount);
                m_ResourceAmount = 0;
            }
            else
            {
                ResourceManager.AddResource(ResourceType, amount);
                m_ResourceAmount -= amount;
            }

            if (m_ResourceAmount <= 0)
            {
                m_Destroyed = true;
                for (int i = 0; i < m_VillagerList.Count; i++)
                {
                    UnitVillager villager = m_VillagerList[i];
                    ResourceManager.DeassignToResource(ResourceType);
                    villager.ResourceDepleted(m_ResourceSO.SearchAfterDeplete);
                }
                m_VillagerList.Clear();
                GetComponent<UnitMember>().DestroyUnit();
            }
        }

        public void AddVillager(UnitVillager villager)
        {
            if (!m_VillagerList.Contains(villager))
            {
                m_VillagerList.Add(villager);
                ResourceManager.AssignToResource(ResourceType);
            }
        }

        public void RemoveVillager(UnitVillager villager)
        {
            if (m_VillagerList.Contains(villager))
            {
                m_VillagerList.Remove(villager);
                ResourceManager.DeassignToResource(ResourceType);
            }
        }
    }
}
