using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class BuildManager : Singleton<BuildManager>
    {
        public static Color CanBuildColor => Instance.m_CanBuildColor;
        public static Color CantBuildColor => Instance.m_CantBuildColor;

        [SerializeField] private bool m_Snap = true;
        [SerializeField] private LayerMask m_BuildableLayer;
        [SerializeField] private LayerMask m_IgnoreLayer;
        [SerializeField] private UnitBuildingSO[] m_Buildings;
        private GhostUnit m_GhostUnit;

        [Header("Building Visuals")]
        [SerializeField] private Color m_CanBuildColor;
        [SerializeField] private Color m_CantBuildColor;

        [Header("UI")]
        [SerializeField] private Transform m_ShopPlaceholder;
        [SerializeField] private BuyUnitButton m_BuyUnitButton;

        private void Awake()
        {
            foreach (var unit in m_Buildings)
            {
                var button = Instantiate(m_BuyUnitButton, m_ShopPlaceholder);
                button.Setup(unit);
            }
        }

        public static void SetBuildObject(UnitBuildingSO unit)
        {
            Instance.CallGhostPrefab(unit);
        }

        public static void DestroyBuildObject()
        {
            Instance.EndBuild();
        }

        public static UnitBuildingSO GetBuildingByName(string name)
        {
            foreach (UnitBuildingSO building in Instance.m_Buildings)
            {
                if (building.UnitName == name)
                    return building;
            }
            return null;
        }

        private void CallGhostPrefab(UnitBuildingSO unit)
        {
            m_GhostUnit = Instantiate(unit.GhostPrefab, Vector3.zero, Quaternion.identity);
            m_GhostUnit.Setup(unit, m_BuildableLayer, m_IgnoreLayer, m_Snap);
        }

        private void EndBuild()
        {
            Destroy(m_GhostUnit.gameObject);
            m_GhostUnit = null;
        }
    }
}
