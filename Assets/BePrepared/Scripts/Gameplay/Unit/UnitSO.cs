using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Create Unit")]
    public class UnitSO : ScriptableObject
    {
        public UnitType UnitType;
        public string UnitName = "Unit";
        public bool Destroyable = true;
    }
}
