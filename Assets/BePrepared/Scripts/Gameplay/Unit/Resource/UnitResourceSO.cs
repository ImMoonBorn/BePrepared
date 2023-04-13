using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [CreateAssetMenu(fileName = "SO_UnitResource", menuName = "BePrepared/Unit/Create Resource")]
    public class UnitResourceSO : UnitSO
    {
        public ResourceType ResourceType;
        public VillagerType VillagerType;
        public bool SearchAfterDeplete = true;
        public float GatherRatePerSecond = 0.0f;
    }
}
