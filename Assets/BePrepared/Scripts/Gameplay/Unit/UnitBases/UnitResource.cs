using System.Collections.Generic;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitResource : MonoBehaviour
    {
        public ResourceType ResourceType => m_ResourceType;
        public int ResourceAmount => m_ResourceAmount;
        public int ResourceAmountMax => m_ResourceAmountMax;

        [SerializeField] private ResourceType m_ResourceType;
        [SerializeField] private int m_ResourceAmountMax = 100;
        private int m_ResourceAmount = 0;

        private readonly List<UnitVillager> m_VillagerList = new();

        private void Awake()
        {
            m_ResourceAmount = m_ResourceAmountMax;
        }

        public void Gather(int amount)
        {
            m_ResourceAmount -= amount;
            if (m_ResourceAmount < 0)
            {
                for (int i = 0; i < m_VillagerList.Count; i++)
                {
                    UnitVillager villager = m_VillagerList[i];
                    villager.ResourceDepleted();
                }
                GetComponent<UnitMember>().DestroyUnit();
            }
        }

        public void AddVillager(UnitVillager villager)
        {
            if (!m_VillagerList.Contains(villager))
            {
                m_VillagerList.Add(villager);
                ResourceManager.AssignToResource(m_ResourceType);
            }
        }

        public void RemoveVillager(UnitVillager villager)
        {
            if (m_VillagerList.Contains(villager))
            {
                m_VillagerList.Remove(villager);
                ResourceManager.DeassignToResource(m_ResourceType);
            }
        }
    }
}
