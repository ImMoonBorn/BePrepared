using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using MoonBorn.Utils;
using MoonBorn.UI;
using MoonBorn.BePrepared.Gameplay.Player;
using MoonBorn.BePrepared.Gameplay.BuildSystem;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitManager : Singleton<UnitManager>
    {
        public UnityEvent OnVillagerCreated;
        public UnityEvent OnVillagerDestroyed;

        public static bool CanProduceVillager => Instance.m_VillagerCount < Instance.m_MaxVillagerCount;
        public static List<UnitVillager> IdleVillagers => Instance.m_IdleVillagers;
        public static int VillagerCount => Instance.m_VillagerCount;
        public static int MaxVillagerCount => Instance.m_MaxVillagerCount;

        public static UnitMember SelectedUnit => Instance.m_SelectedUnit;
        public static UnitVillager SelectedVillager => Instance.m_SelectedVillager;

        [Header("References")]
        private Camera m_Camera;

        [Header("Villagers")]
        [SerializeField] private UnitVillager m_VillagerPrefab;
        [SerializeField] private int m_MaxVillagerCount = 5;
        [SerializeField] private TMP_Text m_VillagerCountText;
        private int m_VillagerCount = 0;
        private readonly List<UnitVillager> m_IdleVillagers = new();

        [Header("Unit Selection")]
        [SerializeField] private LayerMask m_SelectableLayer;
        [SerializeField] private LayerMask m_MoveableLayer;
        private UnitMember m_SelectedUnit;
        private UnitVillager m_SelectedVillager;

        private void Awake()
        {
            m_Camera = Camera.main;

            UpdateVillagerUI();
        }

        private void Start()
        {
            UnitUI.ChangeIdleVillagers(0);
        }

        public void SelectUnit(UnitMember unit)
        {
            if (unit.transform.TryGetComponent(out UnitVillager controller))
                m_SelectedVillager = controller;
            else
                m_SelectedVillager = null;

            if (m_SelectedUnit != unit)
            {
                if (m_SelectedUnit != null)
                    m_SelectedUnit.Deselect();
                unit.Select();
            }

            m_SelectedUnit = unit;
        }

        public void DeselectUnit()
        {
            if (m_SelectedUnit != null)
            {
                m_SelectedUnit.Deselect();
                m_SelectedUnit = null;

                if (m_SelectedVillager != null)
                    m_SelectedVillager = null;

                if (CameraController.IsFocused)
                    CameraController.Unfocus();
            }
        }

        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject() || GameManager.MouseState != MouseState.Idle)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(m_Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, m_SelectableLayer))
                {
                    if (hit.transform.TryGetComponent(out UnitMember unit))
                        SelectUnit(unit);
                    else
                        DeselectUnit();
                }
                else
                    DeselectUnit();
            }
            else if (Input.GetMouseButtonDown(1) && m_SelectedVillager)
            {
                if (Physics.Raycast(m_Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, m_MoveableLayer | m_SelectableLayer))
                {
                    if (hit.transform.TryGetComponent(out UnitResource resource))
                    {
                        m_SelectedVillager.Assign(resource, hit.collider.ClosestPoint(m_SelectedVillager.transform.position));
                    }
                    else if (hit.transform.TryGetComponent(out UnitConstruction construction))
                    {
                        m_SelectedVillager.Move(hit.collider.ClosestPoint(m_SelectedVillager.transform.position));
                        m_SelectedVillager.AssignBuilder(construction);
                    }
                    else
                    {
                        m_SelectedVillager.Move(hit.point);
                        m_SelectedVillager.Unassign();
                    }
                }
            }

            if (m_SelectedUnit)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (CameraController.IsFocused)
                        CameraController.Unfocus();
                    else
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                            CameraController.FocusTarget(m_SelectedUnit.transform);
                        else
                            CameraController.MoveToTarget(m_SelectedUnit.transform.position);
                    }
                }

            }
        }

        #region Villager Population stuff

        public static UnitVillager CreateVillager(Vector3 position, Vector3 eulerAngles = new Vector3())
        {
            if (!CanProduceVillager)
            {
                NotificationManager.Notificate("Population Limit Reached!", NotificationType.Warning);
                return null;
            }

            UnitVillager villager = Instantiate(Instance.m_VillagerPrefab, position, Quaternion.Euler(eulerAngles));
            return villager;
        }

        private void UpdateVillagerUI()
        {
            if (m_VillagerCountText != null)
                m_VillagerCountText.text = $"{m_VillagerCount} / {m_MaxVillagerCount}";
        }

        public static void VillagerCreated()
        {
            if (Instance == null)
                return;

            Instance.OnVillagerCreated?.Invoke();
            Instance.m_VillagerCount++;
            Instance.UpdateVillagerUI();
        }

        public static void VillagerDestroyed()
        {
            if (Instance == null)
                return;

            Instance.OnVillagerDestroyed?.Invoke();
            Instance.m_VillagerCount--;
            Instance.UpdateVillagerUI();
        }

        public static void AddIdleVillager(UnitVillager villager)
        {
            if (Instance == null)
                return;

            if (!Instance.m_IdleVillagers.Contains(villager))
            {
                Instance.m_IdleVillagers.Add(villager);
                UnitUI.ChangeIdleVillagers(Instance.m_IdleVillagers.Count);
            }

        }

        public static void RemoveIdleVillager(UnitVillager villager)
        {
            if (Instance.m_IdleVillagers.Contains(villager))
            {
                Instance.m_IdleVillagers.Remove(villager);
                UnitUI.ChangeIdleVillagers(Instance.m_IdleVillagers.Count);
            }
        }

        public static void IncreasePopulationLimit(int amount)
        {
            Instance.m_MaxVillagerCount += amount;
            Instance.UpdateVillagerUI();
        }

        public static void DecreasePopulationLimit(int amount)
        {
            if (Instance == null)
                return;

            Instance.m_MaxVillagerCount -= amount;
            Instance.UpdateVillagerUI();
        }
        #endregion
    }
}
