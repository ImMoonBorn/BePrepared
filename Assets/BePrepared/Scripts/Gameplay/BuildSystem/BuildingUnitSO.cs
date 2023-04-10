using UnityEngine;
using MoonBorn.BePrepared.Gameplay.Unit;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    [CreateAssetMenu(fileName = "SO_BuildingUnit", menuName = "BePrepared/Unit/Create Building")]
    public class BuildingUnitSO : UnitSO
    {
        [Header("Tooltip")]
        public Sprite Icon;
        [Multiline()]
        public string Description;

        [Header("Build Settings")]
        public ResourceCost Cost;
        public Vector3 ColliderSize;
        public float BuildTime;
        public float BuildHeight;

        [Header("Prefabs")]
        public GhostUnit GhostPrefab;
        public UnitConstruction ConstructionPrefab;
        public GameObject FinishedPrefab;
    }
}