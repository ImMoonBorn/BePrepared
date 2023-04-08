using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class BuildingHouse : UnitBuilding
    {
        [SerializeField] private int m_PopulationHosting = 5;

        private void Start()
        {
            UnitManager.IncreasePopulationLimit(m_PopulationHosting);
        }

        private void OnDestroy()
        {
            UnitManager.DecreasePopulationLimit(m_PopulationHosting);
        }
    }
}
