using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Utils.SaveSystem;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [System.Serializable]
    public class ImprovementProp
    {
        public bool IsDone => m_IsDone;
        public bool IsImproving => m_IsImproving;
        public string BuildingGUID => m_ImprovingUnitGUID;
        public float Progress => m_Progress;
        public void SetImproving(bool improving) => m_IsImproving = improving;
        public void SetBuildingGUID(string buildingGUID) => m_ImprovingUnitGUID = buildingGUID;
        public void SetProgress(float progress) => m_Progress = progress;

        private bool m_IsDone = false;
        private bool m_IsImproving = false;
        private string m_ImprovingUnitGUID = string.Empty;
        private float m_Progress = 0.0f;

        public ImprovementSO ImprovementSO;

        public void Improve()
        {
            m_IsDone = true;
            switch (ImprovementSO.VillagerType)
            {
                case VillagerType.Lumberjack: UnitImprovements.SetLumberjackSpeed(ImprovementSO.Amount); break;
                case VillagerType.Farmer: UnitImprovements.SetFarmerSpeed(ImprovementSO.Amount); break;
                case VillagerType.Miner: UnitImprovements.SetMinerSpeed(ImprovementSO.Amount); break;
            }
        }
    }

    public class ImprovementManager : Singleton<ImprovementManager>
    {
        public static List<ImprovementProp> Improvements => Instance.m_Improvements;

        [SerializeField] private List<ImprovementProp> m_Improvements;

        public static ImprovementProp FindImprovement(ImprovementSO so)
        {
            foreach (ImprovementProp prop in Instance.m_Improvements)
                if (so == prop.ImprovementSO)
                    return prop;

            return null;
        }

        public static ImprovementProp FindImprovement(string name)
        {
            foreach (ImprovementProp prop in Instance.m_Improvements)
                if (name == prop.ImprovementSO.Name)
                    return prop;

            return null;
        }
    }
}
