using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [CreateAssetMenu(fileName = "SO_Improvement", menuName = "BePrepared/Improvement/Create Improvement")]
    public class ImprovementSO : ScriptableObject
    {
        public VillagerType VillagerType;
        public string Name;
        public string Description;
        [Range(0.0f, 1.0f)] public float Amount;
        public float Time;
        public Sprite Icon;
        public ResourceCost Cost;
        public ImprovementSO UnlockImprovement;
    }
}
