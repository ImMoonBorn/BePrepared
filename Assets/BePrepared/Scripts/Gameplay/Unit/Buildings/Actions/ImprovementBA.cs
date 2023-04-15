using MoonBorn.Utils;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class ImprovementBA : BuildingAction
    {
        [Header("Improvement Settings")]
        [SerializeField] private GUIDComponent m_BuildingGUID;
        [SerializeField] private ImprovementSO m_ImprovementSO;
        private ImprovementProp m_ImprovementProp;

        private void Start()
        {
            Refresh();
            if (m_ImprovementProp != null && m_ImprovementProp.IsImproving)
                ContinueAction(m_ImprovementProp.Progress);
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
            return $"{m_ImprovementSO.VillagerType}s gather {UnitVillager.GetResourceTypeFromVillager(m_ImprovementSO.VillagerType)} {m_ImprovementSO.Amount * 100.0f}% faster";
        }

        protected override string CustomFooter()
        {
            string footer = string.Empty;
            if (m_ImprovementSO.UnlockImprovement)
                footer = "Unlocks new improvement.";
            return footer;
        }

        protected override void OnActionCall()
        {
            m_ImprovementProp.SetImproving(true);
            m_ImprovementProp.SetBuildingGUID(m_BuildingGUID.GUID);
        }

        protected override void OnActionUpdate()
        {
            m_ImprovementProp.SetProgress(m_ActionTimer);
        }

        protected override void OnActionFinish()
        {
            m_ImprovementProp.Improve();
            m_ImprovementProp.SetImproving(false);
            m_ImprovementProp.SetBuildingGUID(string.Empty);
            m_ImprovementProp.SetProgress(0.0f);

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

        protected override void OnActionCancel()
        {
            m_ImprovementProp.SetImproving(false);
            m_ImprovementProp.SetBuildingGUID(string.Empty);
            m_ImprovementProp.SetProgress(0.0f);
        }

        public override void Refresh()
        {
            m_ImprovementProp = ImprovementManager.FindImprovement(m_ImprovementSO);

            if (m_ImprovementProp != null)
            {
                if (m_ImprovementProp.IsImproving && m_ImprovementProp.BuildingGUID != m_BuildingGUID.GUID)
                {
                    m_DestroyOnLimitReach = true;
                    m_ReachedLimit = true;
                    return;
                }
                else if (!m_ImprovementProp.IsImproving)
                {
                    m_DestroyOnLimitReach = false;
                    m_ReachedLimit = false;
                }

                if (m_ImprovementProp.IsDone)
                {
                    m_ImprovementSO = m_ImprovementSO.UnlockImprovement;
                    if (m_ImprovementSO == null)
                    {
                        m_DestroyOnLimitReach = true;
                        m_ReachedLimit = true;
                        return;
                    }

                    m_ImprovementProp = ImprovementManager.FindImprovement(m_ImprovementSO);
                    Refresh();
                    return;
                }
            }
            else
            {
                m_DestroyOnLimitReach = true;
                m_ReachedLimit = true;
                return;
            }
            OverrideAction(m_ImprovementSO.Name, m_ImprovementSO.Time, m_ImprovementSO.Cost, m_ImprovementSO.Icon);
        }
    }
}
