using UnityEngine;
using MoonBorn.Utils;
using TMPro;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public enum UnitCategory
    {
        Unit = 0,
        Prop = 1,
    }

    public class BuildManager : Singleton<BuildManager>
    {
        public static Color CanBuildColor => Instance.m_CanBuildColor;
        public static Color CantBuildColor => Instance.m_CantBuildColor;

        [SerializeField] private bool m_Snap = true;
        [SerializeField] private LayerMask m_BuildableLayer;
        [SerializeField] private LayerMask m_IgnoreLayer;
        [SerializeField] private UnitBuildingSO[] m_Buildings;
        private GhostUnit m_GhostUnit;
        private UnitCategory m_UnitCategory;

        [Header("Building Visuals")]
        [SerializeField] private Color m_CanBuildColor;
        [SerializeField] private Color m_CantBuildColor;

        [Header("UI")]
        [SerializeField] private Transform m_UnitPlaceholder;
        [SerializeField] private Transform m_PropPlaceholder;
        [SerializeField] private BuyUnitButton m_BuyUnitButton;
        [SerializeField] private TMP_Dropdown m_CategorySelector;

        private void Awake()
        {
            m_UnitCategory = UnitCategory.Unit;
            m_UnitPlaceholder.gameObject.SetActive(true);
            m_PropPlaceholder.gameObject.SetActive(false);

            foreach (var unit in m_Buildings)
            {
                Transform placeholder = unit.UnitCategory switch
                {
                    UnitCategory.Unit => m_UnitPlaceholder,
                    UnitCategory.Prop => m_PropPlaceholder,
                    _ => null,
                };

                var button = Instantiate(m_BuyUnitButton, placeholder);
                button.Setup(unit);
            }

            m_CategorySelector.onValueChanged.AddListener((int index) =>
            {
                m_UnitCategory = (UnitCategory)index;

                m_UnitPlaceholder.gameObject.SetActive(false);
                m_PropPlaceholder.gameObject.SetActive(false);

                switch (m_UnitCategory)
                {
                    case UnitCategory.Unit: m_UnitPlaceholder.gameObject.SetActive(true); break;
                    case UnitCategory.Prop: m_PropPlaceholder.gameObject.SetActive(true); break;
                    default: break;
                }
            });
        }

        public static void SetBuildObject(UnitBuildingSO unit)
        {
            Instance.CallGhostPrefab(unit);
        }

        private void CallGhostPrefab(UnitBuildingSO unit)
        {
            m_GhostUnit = Instantiate(unit.GhostPrefab, Vector3.zero, Quaternion.identity);
            m_GhostUnit.Setup(unit, m_BuildableLayer, m_IgnoreLayer, m_Snap);

            GameManager.SetMouseState(MouseState.Building);
        }

        public static void DestroyBuildObject()
        {
            Instance.EndBuild();
        }

        private void EndBuild()
        {
            Destroy(m_GhostUnit.gameObject);
            m_GhostUnit = null;

            GameManager.RestoreMouseState();
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
    }
}
