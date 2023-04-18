using UnityEngine;
using UnityEngine.UI;
using MoonBorn.UI;
using MoonBorn.BePrepared.Gameplay.Unit;
using UnityEditor.Rendering.LookDev;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class BuyUnitButton : MonoBehaviour
    {
        [SerializeField] private Image m_Icon;
        private UnitBuildingSO m_UnitSO;
        private Button m_Button;
        private TooltipTrigger m_TooltipTrigger;

        public void Setup(UnitBuildingSO unit)
        {
            m_Button = GetComponent<Button>();
            m_TooltipTrigger = GetComponent<TooltipTrigger>();

            m_Button.onClick.AddListener(Buy);
            m_UnitSO = unit;

            m_Icon.sprite = unit.Icon;

            if (m_TooltipTrigger != null)
                m_TooltipTrigger.OnTriggerEnter += () => m_TooltipTrigger.SetTooltip(GenerateDescription(), unit.UnitName);

            RefreshButton();

            ResourceManager.OnResourceChange.AddListener(RefreshButton);
        }

        private string GenerateDescription()
        {
            string costText = "";
            foreach (CostProps c in m_UnitSO.Cost.CostProps)
            {
                if (ResourceManager.CheckForResource(c.ResourceType, c.Amount))
                    costText += $"-{c.ResourceType}: {c.Amount}\n";
                else
                    costText += $"-<color=red>{c.ResourceType}: {c.Amount}</color>\n";
            }

            string consumptionText = "";
            foreach (CostProps c in m_UnitSO.Consumptions.CostProps)
            {
                if (ResourceManager.CheckForResource(c.ResourceType, c.Amount))
                    consumptionText += $"-{c.ResourceType}: {c.Amount}\n";
                else
                    consumptionText += $"-<color=red>{c.ResourceType}: {c.Amount}</color>\n";
            }

            string description = m_UnitSO.Description;
            
            if (!string.IsNullOrEmpty(costText))
                description += $"\n\nCost:\n{costText}";

            if (!string.IsNullOrEmpty(consumptionText))
                description += $"\nConsumptions:\n{consumptionText}";
            
            return description;
        }

        private void RefreshButton()
        {
            m_Button.interactable = m_UnitSO.Cost.Check();
        }

        public void Buy()
        {
            if (!m_UnitSO.Cost.Check())
            {
                NotificationManager.Notificate("Not enough resources to build!", NotificationType.Warning);
                return;
            }
            m_UnitSO.Cost.Spend();
            BuildManager.SetBuildObject(m_UnitSO);
        }

        private void OnDestroy()
        {
            ResourceManager.OnResourceChange.RemoveListener(RefreshButton);
        }
    }
}
