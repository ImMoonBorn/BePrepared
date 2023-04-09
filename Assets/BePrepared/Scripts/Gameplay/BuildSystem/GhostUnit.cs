using MoonBorn.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class GhostUnit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_SelectionHUD;
        private BuildingUnitSO m_UnitSO;

        private LayerMask m_BuildableLayer;
        private LayerMask m_IgnoreLayer;
        private Vector3 m_MousePosition = Vector3.zero;
        private bool m_Snap = true;

        public void Setup(BuildingUnitSO unitSO, LayerMask buildableLayer, LayerMask ignoreLayer, bool snap)
        {
            m_UnitSO = unitSO;
            m_BuildableLayer = buildableLayer;
            m_IgnoreLayer = ignoreLayer;
            m_Snap = snap;
            m_MousePosition = UtilityFunctions.GetWorldMousePosition(m_BuildableLayer, true, m_MousePosition);
            if (m_Snap)
                m_MousePosition.Set(Mathf.Round(m_MousePosition.x), 0.0f, Mathf.Round(m_MousePosition.z));
            transform.position = m_MousePosition;
        }

        private void Update()
        {
            m_MousePosition = UtilityFunctions.GetWorldMousePosition(m_BuildableLayer, true, m_MousePosition);
            if (m_Snap)
                m_MousePosition.Set(Mathf.Round(m_MousePosition.x), 0.0f, Mathf.Round(m_MousePosition.z));

            Vector3 colliderSize = m_UnitSO.ColliderSize * 0.5f;
            bool cantBuild = Physics.CheckBox(transform.position, colliderSize, Quaternion.identity, ~m_IgnoreLayer);

            m_SelectionHUD.color = cantBuild ? BuildManager.CantBuildColor : BuildManager.CanBuildColor;
            
            if (Input.GetMouseButtonDown(0) && !cantBuild && !EventSystem.current.IsPointerOverGameObject())
            {
                UnitConstruction unit = Instantiate(m_UnitSO.ConstructionPrefab, m_MousePosition, Quaternion.identity);
                unit.Setup(m_UnitSO);
                BuildManager.DestroyBuildObject();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                m_UnitSO.Cost.Restore();
                BuildManager.DestroyBuildObject();
            }
        }

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, m_MousePosition, Time.deltaTime * 8.0f);
        }
    }
}
