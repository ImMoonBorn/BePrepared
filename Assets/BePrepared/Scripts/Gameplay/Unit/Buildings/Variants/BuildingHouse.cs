using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class BuildingHouse : UnitBuilding
    {
        [SerializeField] private int m_PopulationHosting = 5;

        protected override void Started()
        {
            UnitManager.IncreasePopulationLimit(m_PopulationHosting);
        }

        protected override void Destroyed()
        {
            UnitManager.DecreasePopulationLimit(m_PopulationHosting);
        }
    }
}
