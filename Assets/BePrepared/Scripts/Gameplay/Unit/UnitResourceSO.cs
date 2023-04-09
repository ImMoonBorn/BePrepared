using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [CreateAssetMenu(fileName = "SO_UnitResource", menuName = "BePrepared/Unit/Create Resource")]
    public class UnitResourceSO : UnitSO
    {
        public ResourceType ResourceType;
        public bool SearchAfterDeplete = true;
    }
}
