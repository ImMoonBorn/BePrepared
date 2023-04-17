using MoonBorn.BePrepared.Gameplay.Unit;
using MoonBorn.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class GhostUnit : MonoBehaviour
    {
        private enum BuildDirection
        {
            Forward, Left, Backward, Right
        }

        [SerializeField] private SpriteRenderer m_SelectionHUD;
        private UnitBuildingSO m_UnitSO;
        private BuildDirection m_Direction = BuildDirection.Forward;

        private LayerMask m_BuildableLayer;
        private LayerMask m_IgnoreLayer;
        private Vector3 m_MousePosition = Vector3.zero;
        private Quaternion m_TargetRotation = Quaternion.identity;
        private bool m_Snap = true;

        public void Setup(UnitBuildingSO unitSO, LayerMask buildableLayer, LayerMask ignoreLayer, bool snap)
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
            Vector3 colliderOffset = m_UnitSO.ColliderOffset;

            if(m_Direction == BuildDirection.Left || m_Direction == BuildDirection.Right)
                colliderOffset = new Vector3(colliderOffset.z, colliderOffset.y, colliderOffset.x);

            bool cantBuild = Physics.CheckBox(transform.position + colliderOffset, colliderSize, Quaternion.identity, ~m_IgnoreLayer);

            m_SelectionHUD.color = cantBuild ? BuildManager.CantBuildColor : BuildManager.CanBuildColor;

            if (Input.GetKeyDown(KeyCode.R))
            {
                m_Direction = ChangeDirection(m_Direction);
                m_TargetRotation = RotateToDirection(m_Direction);
            }

            if (Input.GetMouseButtonDown(0) && !cantBuild && !EventSystem.current.IsPointerOverGameObject())
            {
                UnitConstruction unit = Instantiate(m_UnitSO.ConstructionPrefab, m_MousePosition, m_TargetRotation);
                unit.Setup(m_UnitSO);
                BuildManager.DestroyBuildObject();

                UnitVillager villager = UnitManager.SelectedVillager;
                if (villager != null)
                    villager.AssignBuilder(unit, unit.GetComponent<Collider>().ClosestPoint(villager.transform.position));
            }
            else if (Input.GetMouseButtonDown(1))
            {
                m_UnitSO.Cost.Restore();
                BuildManager.DestroyBuildObject();
            }
        }

        private void LateUpdate()
        {
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, m_MousePosition, Time.deltaTime * 8.0f), Quaternion.Lerp(transform.rotation, m_TargetRotation, Time.deltaTime * 8.0f));
        }

        private Quaternion RotateToDirection(BuildDirection direction)
        {
            return direction switch
            {
                BuildDirection.Forward => Quaternion.Euler(0.0f, 0.0f, 0.0f),
                BuildDirection.Backward => Quaternion.Euler(0.0f, 180.0f, 0.0f),
                BuildDirection.Left => Quaternion.Euler(0.0f, 90.0f, 0.0f),
                BuildDirection.Right => Quaternion.Euler(0.0f, 270.0f, 0.0f),
                _ => Quaternion.identity
            };
        }

        private BuildDirection ChangeDirection(BuildDirection direction)
        {
            return direction switch
            {
                BuildDirection.Forward => BuildDirection.Left,
                BuildDirection.Left => BuildDirection.Backward,
                BuildDirection.Backward => BuildDirection.Right,
                BuildDirection.Right => BuildDirection.Forward,
                _ => BuildDirection.Forward
            };
        }
    }
}
