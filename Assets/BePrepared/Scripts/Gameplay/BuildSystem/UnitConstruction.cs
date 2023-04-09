using System.Collections.Generic;
using UnityEngine;
using MoonBorn.BePrepared.Gameplay.Unit;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class UnitConstruction : MonoBehaviour
    {
        [SerializeField] private GameObject m_Mesh;
        private BuildingUnitSO m_UnitSO;
        
        private readonly List<UnitVillager> m_Villagers = new();
        private readonly List<Material> m_Materials = new();
        private float m_Timer = 0.0f;
        private int m_Builders = 0;

        public void Setup(BuildingUnitSO unit)
        {
            m_UnitSO = unit;
            MeshRenderer[] renderers = m_Mesh.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
                m_Materials.Add(renderer.material);
            foreach (Material material in m_Materials)
                material.SetVector("_DissolveOffest", new Vector4(0, 0, 0, 0));
        }

        private void Update()
        {
            if (m_Builders > 0)
            {
                float deltaTime = Time.deltaTime;
                m_Timer += deltaTime + ((m_Builders - 1) * (0.4f * deltaTime));

                foreach (Material material in m_Materials)
                    material.SetVector("_DissolveOffest", new Vector4(0, (m_Timer / m_UnitSO.BuildTime) * m_UnitSO.BuildHeight, 0, 0));

                if (m_Timer >= m_UnitSO.BuildTime)
                {
                    for (int i = 0; i < m_Villagers.Count; i++)
                    {
                        UnitVillager villager = m_Villagers[i];
                        villager.BuildFinished();
                    }

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
            if (m_Timer <= 0.0f)
                m_UnitSO.Cost.Restore();
        }
    }
}
