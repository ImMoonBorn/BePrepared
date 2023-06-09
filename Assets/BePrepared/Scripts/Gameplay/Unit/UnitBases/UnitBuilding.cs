using UnityEngine;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.BuildSystem;
using MoonBorn.BePrepared.Gameplay.Consumption;
using MoonBorn.BePrepared.Utils.SaveSystem;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitBuilding : MonoBehaviour, ISaveable
    {
        public BuildingAction[] Actions => m_Actions;

        [SerializeField] private UnitBuildingSO m_BuildingSO;
        [SerializeField] private Transform m_ActionPlaceholder;
        private BuildingAction[] m_Actions;

        [SerializeField] private GUIDComponent[] m_HasResources;

        protected virtual void Started() { }
        protected virtual void Destroyed() { }


        private void Awake()
        {
            m_Actions = m_ActionPlaceholder.GetComponentsInChildren<BuildingAction>();
        }

        private void Start()
        {
            foreach (BuildingAction action in m_Actions)
                action.Refresh();

            foreach (CostProps cost in m_BuildingSO.Consumptions.CostProps)
                ConsumptionManager.AddConsumptions(cost.ResourceType, cost.Amount);

            Started();
        }

        public void SetResources(string[] resources)
        {
            for (int i = 0; i < m_HasResources.Length; i++)
                m_HasResources[i].SetGuid(resources[i]);
        }

        public void SaveState(string guid)
        {
            string[] resources = new string[m_HasResources.Length];

            for (int i = 0; i < resources.Length; i++)
                resources[i] = m_HasResources[i].GUID;

            BuildingData data = new BuildingData
            {
                GUID = guid,
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles,
                BuildingUnitName = m_BuildingSO.UnitName,
                HasResoures = resources
            };

            SaveManager.SaveToBuildingData(data);
        }

        public void LoadState(object saveData)
        {
        }

        private void OnDestroy()
        {
            foreach (CostProps cost in m_BuildingSO.Consumptions.CostProps)
                ConsumptionManager.RemoveConsumptions(cost.ResourceType, cost.Amount);

            Destroyed();
        }
    }
}
