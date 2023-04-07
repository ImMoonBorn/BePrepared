using System.Collections;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class ProduceVillagerBA : BuildingAction
    {
        [Header("Spawn Options")]
        [SerializeField] private Transform m_SpawnLocation;
        [SerializeField] private Transform m_MoveLocation;

        protected override bool CustomCondition()
        {
            return UnitManager.CanProduceVillager;
        }

        protected override string CustomConditionWarningMessage()
        {
            return "Reached Population Limit";
        }

        protected override void OnAction()
        {
            UnitManager.CreateVillager(m_SpawnLocation.position, m_MoveLocation.position);
        }
    }
}
