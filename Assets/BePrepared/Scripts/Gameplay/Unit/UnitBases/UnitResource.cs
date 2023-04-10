using System.Collections.Generic;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitResource : MonoBehaviour
    {
        public ResourceType ResourceType => m_ResourceSO.ResourceType;
        public int ResourceAmount => m_ResourceAmount;
        public int ResourceAmountMax => m_ResourceAmountMax;
        public bool IsInfinite => m_IsInfinite;

        [SerializeField] private UnitResourceSO m_ResourceSO;
        [SerializeField] private bool m_IsInfinite = false;
        [SerializeField] private int m_ResourceAmountMax = 100;

        private int m_ResourceAmount = 0;
        private bool m_Destroyed = false;
        private List<UnitVillager> m_VillagerList = new();

        [Header("Random Rotation")]
        [SerializeField] private bool m_ApplyRandomRotation = true;
        [SerializeField] private Transform m_MeshTransform;


        private void Awake()
        {
            m_ResourceAmount = m_ResourceAmountMax;
            if (m_ApplyRandomRotation)
            {
                Vector3 euler = m_MeshTransform.rotation.eulerAngles;
                m_MeshTransform.rotation = Quaternion.Euler(euler.x, Random.Range(0.0f, 360.0f), euler.z);
            }
        }

        public void Gather(int amount)
        {
            if (m_Destroyed)
                return;

            if (m_IsInfinite)
            {
                ResourceManager.AddResource(ResourceType, amount);
                return;
            }

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

        private void OnDestroy()
        {
            for (int i = 0; i < m_VillagerList.Count; i++)
            {
                UnitVillager villager = m_VillagerList[i];
                ResourceManager.DeassignToResource(ResourceType);
                villager.ResourceDepleted(m_ResourceSO.SearchAfterDeplete);
            }
            m_VillagerList.Clear();
        }
    }
}
