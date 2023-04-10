using UnityEngine;
using UnityEngine.Analytics;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    [CreateAssetMenu(fileName = "SO_UnitVillager", menuName = "BePrepared/Unit/Create Villager")]
    public class UnitVillagerSO : UnitSO
    {
        [Header("Audio")]
        public AudioClip[] MaleExpressions;
        public AudioClip[] FemaleExpressions;

        public AudioClip GetClipByGender(VillagerGender gender)
        {
            return gender switch
            {
                VillagerGender.Male => MaleExpressions[Random.Range(0, MaleExpressions.Length - 1)],
                VillagerGender.Female => FemaleExpressions[Random.Range(0, FemaleExpressions.Length - 1)],
                _ => null
            };
        }
    }
}
