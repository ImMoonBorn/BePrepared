using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [System.Serializable]
    public struct CostProps
    {
        public ResourceType ResourceType;
        public int Amount;
    }

    [System.Serializable]
    public class ResourceCost
    {
        public CostProps[] CostProps => m_Costs;

        [SerializeField] private CostProps[] m_Costs;

        public void Spend()
        {
            foreach(CostProps c in m_Costs)
                ResourceManager.SpendResource(c.ResourceType, c.Amount);
        }

        public void Restore()
        {
            foreach (CostProps c in m_Costs)
                ResourceManager.SpendResource(c.ResourceType, -c.Amount);
        }

        public bool Check()
        {
            bool isEnough = true;
            foreach (CostProps c in m_Costs)
            {
                if (!ResourceManager.CheckForResource(c.ResourceType, c.Amount))
                {
                    isEnough = false;
                    break;
                }    
            }
            return isEnough;
        }
    }
}
