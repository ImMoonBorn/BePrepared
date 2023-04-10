using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class ImprovementBA : BuildingAction
    {
        [Header("Improvement Settings")]
        [SerializeField] private ImprovementSO m_ImprovementSO;
        private ImprovementProp m_ImprovementProp;

        private void Start()
        {
            Refresh();
        }

        protected override bool CustomCondition()
        {
            return true;
        }

        protected override string CustomConditionWarningMessage()
        {
            return "";
        }

        protected override string CustomDescription()
        {
            return "";
        }

        protected override string CustomFooter()
        {
            return "";
        }

        protected override void OnAction()
        {
            m_ImprovementProp.Improve();

            if (m_ImprovementSO.UnlockImprovement != null)
            {
                m_ImprovementSO = m_ImprovementSO.UnlockImprovement;
                OverrideAction(m_ImprovementSO.Name, m_ImprovementSO.Time, m_ImprovementSO.Cost, m_ImprovementSO.Icon);
                UnitUI.RefreshUnit();
            }
            else
            {
                m_DestroyOnLimitReach = true;
                m_ReachedLimit = true;
                UnitUI.RefreshUnit();
            }
        }

        public override void Refresh()
        {
            m_ImprovementProp = ImprovementManager.FindImprovement(m_ImprovementSO);

            if (m_ImprovementProp.IsDone)
            {
                if (m_ImprovementSO.UnlockImprovement != null)
                {
                    m_ImprovementSO = m_ImprovementSO.UnlockImprovement;
                    OverrideAction(m_ImprovementSO.Name, m_ImprovementSO.Time, m_ImprovementSO.Cost, m_ImprovementSO.Icon);
                    return;
                }

                m_DestroyOnLimitReach = true;
                m_ReachedLimit = true;
                return;
            }
            OverrideAction(m_ImprovementSO.Name, m_ImprovementSO.Time, m_ImprovementSO.Cost, m_ImprovementSO.Icon);
        }
    }
}
