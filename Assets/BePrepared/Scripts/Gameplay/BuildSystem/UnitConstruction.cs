using System.Collections.Generic;
using UnityEngine;
using MoonBorn.BePrepared.Gameplay.Unit;
using MoonBorn.BePrepared.Utils.SaveSystem;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class UnitConstruction : MonoBehaviour, ISaveable
    {
        [SerializeField] private GameObject m_Mesh;
        private UnitBuildingSO m_UnitSO;

        private readonly List<UnitVillager> m_Villagers = new();
        private readonly List<Material> m_Materials = new();
        private float m_BuildedAmount = 0.0f;
        private int m_Builders = 0;

        public void Setup(UnitBuildingSO unit)
        {
            m_UnitSO = unit;
            MeshRenderer[] renderers = m_Mesh.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
                m_Materials.Add(renderer.material);
            foreach (Material material in m_Materials)
                material.SetVector("_DissolveOffest", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        private void Update()
        {
            if (m_Builders > 0)
            {
                float deltaTime = Time.deltaTime;
                m_BuildedAmount += deltaTime + ((m_Builders - 1) * (0.4f * deltaTime));

                foreach (Material material in m_Materials)
                    material.SetVector("_DissolveOffest", new Vector4(0, (m_BuildedAmount / m_UnitSO.BuildTime) * (m_UnitSO.BuildHeight), 0.0f, 0.0f));

                if (m_BuildedAmount >= m_UnitSO.BuildTime)
                {
                    Instantiate(m_UnitSO.FinishedPrefab, transform.position, Quaternion.identity);
                    GetComponent<UnitMember>().DestroyUnit();
                }
            }
            m_Builders = 0;
        }

        public void AddVillager(UnitVillager villager)
        {
            if (!m_Villagers.Contains(villager))
                m_Villagers.Add(villager);
        }

        public void RemoveVillager(UnitVillager villager)
        {
            if (m_Villagers.Contains(villager))
                m_Villagers.Remove(villager);
        }

        public void Build()
        {
            m_Builders += 1;
        }

        private void OnDestroy()
        {
            if (m_BuildedAmount <= 0.0f)
                m_UnitSO.Cost.Restore();

            for (int i = 0; i < m_Villagers.Count; i++)
            {
                UnitVillager villager = m_Villagers[i];
                villager.BuildFinished();
            }
        }

        public void SaveState(string guid)
        {
            ConstructionData data = new ConstructionData
            {
                GUID = guid,
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles,
                ConstractionUnitName = m_UnitSO.UnitName,
                BuildedAmount = m_BuildedAmount,
            };

            SaveManager.SaveToConstructionData(data);
        }

        public void LoadState(object saveData)
        {
            ConstructionData constructionData = (ConstructionData)saveData;
            m_BuildedAmount = constructionData.BuildedAmount;

            foreach (Material material in m_Materials)
                material.SetVector("_DissolveOffest", new Vector4(0, (m_BuildedAmount / m_UnitSO.BuildTime) * m_UnitSO.BuildHeight, 0.0f, 0.0f));
        }
    }
}
