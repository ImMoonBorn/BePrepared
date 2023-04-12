using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class ProduceVillagerBA : BuildingAction
    {
        [Header("Spawn Options")]
        [SerializeField] private Transform m_SpawnLocation;

        protected override bool CustomCondition()
        {
            return UnitManager.CanProduceVillager;
        }

        protected override string CustomConditionWarningMessage()
        {
            return "Population Limit Reached!";
        }

        protected override string CustomDescription()
        {
            return "";
        }

        protected override string CustomFooter()
        {
            string desc;
            if (UnitManager.CanProduceVillager)
                desc = $"Created: {UnitManager.VillagerCount}/{UnitManager.MaxVillagerCount}";
            else
                desc = $"-<color=red>Created: {UnitManager.VillagerCount}/{UnitManager.MaxVillagerCount}</color>";

            return desc;
        }

        protected override void OnAction()
        {
            UnitManager.CreateVillager(m_SpawnLocation.position);
        }

        public override void Refresh()
        {
        }
    }
}
