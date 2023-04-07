using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoonBorn.UI;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.Player;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitUI : Singleton<UnitUI>
    {
        private UnitMember m_SelectedUnit;
        private UnitVillager m_SelectedVillager;
        private UnitBuilding m_SelectedBuilding;
        private UnitResource m_SelectedResource;

        [Header("Unit Selection")]
        [SerializeField] private GameObject m_UnitSelect;
        [SerializeField] private GameObject m_DestroyButton;
        [SerializeField] private TMP_Text m_UnitName;
        [SerializeField] private Button m_IdleVillagersButton;
        [SerializeField] private TMP_Text m_IdleVillagersText;
        private int m_IdleIndex = 0;

        [Header("Villager Units")]
        [SerializeField] private GameObject m_VillagerContent;
        [SerializeField] private TMP_Text m_VillagerTypeText;

        [Header("Building Units")]
        [SerializeField] private GameObject m_BuildingContent;
        [SerializeField] private Transform m_ActionButtonPlaceholder;
        [SerializeField] private ActionButton m_ActionButtonPrefab;

        [Header("Resource Units")]
        [SerializeField] private GameObject m_ResourceContent;
        [SerializeField] private TMP_Text m_ResourceAmountText;
        [SerializeField] private Image m_ResourceIcon;

        private void Awake()
        {
            m_UnitSelect.SetActive(false);

            m_DestroyButton.GetComponent<Button>().onClick.AddListener(DestroyUnit);

            m_VillagerContent.SetActive(false);
            m_ResourceContent.SetActive(false);

            m_IdleVillagersButton.onClick.AddListener(ShowIdleVillager);
        }

        public static void Select(UnitMember unit)
        {
            Instance.m_SelectedUnit = unit;

            Instance.SelectUnit();
        }

        public static void Deselect()
        {
            Instance.m_UnitSelect.SetActive(false);
            Instance.m_SelectedUnit = null;

            Instance.m_VillagerContent.SetActive(false);
            Instance.m_BuildingContent.SetActive(false);
            Instance.m_ResourceContent.SetActive(false);

            foreach (Transform child in Instance.m_ActionButtonPlaceholder)
                Destroy(child.gameObject);
        }

        public static void DeselectIfSelected(UnitMember member)
        {
            if (Instance.m_SelectedUnit == member)
                Deselect();
        }

        public static void ChangeIdleVillagers(int count)
        {
            if (count == 0)
                Instance.m_IdleVillagersButton.gameObject.SetActive(false);
            else
            {
                Instance.m_IdleVillagersButton.gameObject.SetActive(true);
                Instance.m_IdleVillagersText.text = count.ToString();
            }
        }

        private void ShowIdleVillager()
        {
            var idleVillagers = UnitManager.IdleVillagers;

            if (m_IdleIndex >= idleVillagers.Count)
                m_IdleIndex = 0;

            CameraController.MoveToTarget(idleVillagers[m_IdleIndex++].transform.position);
        }

        private void SelectUnit()
        {
            UnitSO selectedSO = m_SelectedUnit.UnitSO;

            m_UnitSelect.SetActive(true);
            m_UnitName.text = selectedSO.UnitName;
            m_DestroyButton.SetActive(selectedSO.Destroyable);

            switch (selectedSO.UnitType)
            {
                case UnitType.Building:
                    {
                        m_SelectedBuilding = m_SelectedUnit.GetComponent<UnitBuilding>();
                        Instance.m_BuildingContent.SetActive(true);

                        foreach (var action in m_SelectedBuilding.Actions)
                        {
                            ActionButton button = Instantiate(m_ActionButtonPrefab, m_ActionButtonPlaceholder);
                            button.Setup(action);
                        }
                        break;
                    }
                case UnitType.Villager:
                    {
                        m_SelectedVillager = m_SelectedUnit.GetComponent<UnitVillager>();
                        m_VillagerContent.SetActive(true);
                        UpdateVillager(m_SelectedVillager);
                        break;
                    }
                case UnitType.Resource:
                    {
                        m_SelectedResource = m_SelectedUnit.GetComponent<UnitResource>();
                        m_ResourceContent.SetActive(true);
                        m_UnitName.text += $" ({m_SelectedResource.ResourceType})";
                        m_ResourceIcon.sprite = ResourceManager.GetIconByType(m_SelectedResource.ResourceType);
                        break;
                    }
            }
        }

        private void Update()
        {
            if (m_SelectedResource != null)
                m_ResourceAmountText.text = $"{m_SelectedResource.ResourceAmount} / {m_SelectedResource.ResourceAmountMax}";
        }

        private void DestroyUnit()
        {
            ConfirmBox.OpenConfirmBox(() => m_SelectedUnit.DestroyUnit(), "Destroy Unit");
        }

        public void UpdateVillager(UnitVillager villager)
        {
            if (m_SelectedVillager != villager)
                return;

            m_VillagerTypeText.text = m_SelectedVillager.VillagerType.ToString();
        }
    }
}
