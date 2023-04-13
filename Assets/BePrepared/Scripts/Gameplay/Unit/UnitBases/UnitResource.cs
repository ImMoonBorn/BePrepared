using System.Collections.Generic;
using UnityEngine;
using MoonBorn.BePrepared.Utils.SaveSystem;
using MoonBorn.Utils;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitResource : MonoBehaviour, ISaveable
    {
        public ResourceType ResourceType => m_ResourceSO.ResourceType;
        public VillagerType VillagerType => m_ResourceSO.VillagerType;
        public int VillagerCount => m_VillagerList.Count;
        public int GathererLimit => m_ResourceSO.GathererLimit;
        public bool ReachedGathererLimit => m_VillagerList.Count >= m_ResourceSO.GathererLimit;
        public int ResourceAmount => (int)m_ResourceAmount;
        public int ResourceAmountMax => m_ResourceAmountMax;
        public float GatherRatePerSecond => m_ResourceSO.GatherRatePerSecond;
        public bool IsInfinite => m_IsInfinite;

        private List<UnitVillager> m_VillagerList = new();

        [SerializeField] private UnitResourceSO m_ResourceSO;
        [SerializeField] private bool m_IsInfinite = false;
        [SerializeField] private int m_ResourceAmountMax = 100;
        private float m_ResourceAmount = 0.0f;
        private bool m_Destroyed = false;

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

        public void Gather(float amount)
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

            if (m_ResourceAmount <= 0.0f)
            {
                m_Destroyed = true;
                GetComponent<UnitMember>().DestroyUnit();
                if (TryGetComponent(out CellComponent cellComponent))
                {
                    Vector2Int pos = cellComponent.Position;
                    ProceduralWorldGenerator.DestroyCell(pos.x, pos.y);
                }
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

        public void SaveState(string guid)
        {
            ResourceData data = new ResourceData
            {
                GUID = guid,
                Position = transform.position,
                Rotation = m_MeshTransform.rotation.eulerAngles,
                ResourceAmount = m_ResourceAmount
            };

            SaveManager.SaveToResourceData(data);
        }

        public void LoadState(object saveData)
        {
            ResourceData resourceData = (ResourceData)saveData;
            transform.position = resourceData.Position;
            m_MeshTransform.rotation = Quaternion.Euler(resourceData.Rotation);
            m_ResourceAmount = resourceData.ResourceAmount;

            if (m_ResourceAmount <= 0)
            {
                m_Destroyed = true;
                GetComponent<UnitMember>().DestroyUnit();
                if (TryGetComponent(out CellComponent cellComponent))
                {
                    Vector2Int pos = cellComponent.Position;
                    ProceduralWorldGenerator.DestroyCell(pos.x, pos.y);
                }
            }
        }
    }
}
