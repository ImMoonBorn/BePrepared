using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitBuilding : MonoBehaviour
    {
        public BuildingAction[] Actions => m_Actions;

        [SerializeField] private Transform m_ActionPlaceholder;
        private BuildingAction[] m_Actions;

        private void Awake()
        {
            m_Actions = m_ActionPlaceholder.GetComponentsInChildren<BuildingAction>();
        }
    }
}
