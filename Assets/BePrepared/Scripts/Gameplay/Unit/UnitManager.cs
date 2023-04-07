using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using MoonBorn.Utils;
using MoonBorn.UI;
using MoonBorn.BePrepared.Gameplay.Player;
using System.Collections.Generic;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitManager : Singleton<UnitManager>
    {
        public static bool CanProduceVillager => Instance.m_VillagerCount < Instance.m_MaxVillagerCount;
        public static List<UnitVillager> IdleVillagers => Instance.m_IdleVillagers;

        private Camera m_Camera;

        [Header("Villagers")]
        [SerializeField] private GameObject m_VillagerPrefab;
        [SerializeField] private int m_MaxVillagerCount = 5;
        [SerializeField] private TMP_Text m_VillagerCountText;
        private int m_VillagerCount = 0;
        private List<UnitVillager> m_IdleVillagers = new();

        [Header("Unit Selection")]
        [SerializeField] private LayerMask m_SelectableLayer;
        private UnitMember m_SelectedUnit;

        [SerializeField] private LayerMask m_MoveableLayer;
        private UnitVillager m_SelectedVillager;

        private void Awake()
        {
            m_Camera = Camera.main;

            UpdateVillagerUI();
            UnitUI.ChangeIdleVillagers(0);
        }

        private void SelectUnit(UnitMember unit)
        {
            if (m_SelectedUnit != unit)
            {
                if (m_SelectedUnit != null)
                    m_SelectedUnit.Deselect();
                unit.Select();
            }

            m_SelectedUnit = unit;
        }

        private void DeselectUnit()
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
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(m_Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, m_SelectableLayer))
                {
                    if (hit.transform.TryGetComponent(out UnitVillager controller))
                        m_SelectedVillager = controller;
                    else
                        m_SelectedVillager = null;

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
                        m_SelectedVillager.Move(hit.collider.ClosestPoint(m_SelectedVillager.transform.position));
                        m_SelectedVillager.Assign(resource);
                    }
                    else
                    {
                        m_SelectedVillager.Move(hit.point);
                        m_SelectedVillager.Unassign();
                    }
                }
            }

            if (m_SelectedUnit && Input.GetKeyDown(KeyCode.F))
            {
                if (CameraController.IsFocused)
                    CameraController.Unfocus();
                else
                    CameraController.FocusTarget(m_SelectedUnit.transform);
            }
        }

        public static void CreateVillager(Vector3 position, Vector3 target)
        {
            if (!CanProduceVillager)
            {
                NotificationManager.Notificate("Population Limit Reached!");
                return;
            }

            if (Instantiate(Instance.m_VillagerPrefab, position, Quaternion.identity).TryGetComponent(out UnitVillager villager))
                villager.Move(target);
        }

        public static void VillagerCreated()
        {
            if (Instance == null)
                return;

            Instance.m_VillagerCount++;
            Instance.UpdateVillagerUI();
        }

        public static void VillagerDestroyed()
        {
            if (Instance == null)
                return;

            Instance.m_VillagerCount--;
            Instance.UpdateVillagerUI();
        }

        public void UpdateVillagerUI()
        {
            if (m_VillagerCountText != null)
                m_VillagerCountText.text = $"{m_VillagerCount} / {m_MaxVillagerCount}";
        }

        public static void AddIdleVillager(UnitVillager villager)
        {
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
    }
}
