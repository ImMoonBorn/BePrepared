using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [System.Serializable]
    public class ImprovementProp
    {
        public bool IsDone => m_IsDone;

        public ImprovementSO ImprovementSO;
        private bool m_IsDone;

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
