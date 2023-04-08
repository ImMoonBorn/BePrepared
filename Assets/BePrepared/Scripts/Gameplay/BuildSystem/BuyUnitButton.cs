using UnityEngine;
using UnityEngine.UI;
using MoonBorn.UI;
using MoonBorn.BePrepared.Gameplay.Unit;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class BuyUnitButton : MonoBehaviour
    {
        [SerializeField] private Image m_Icon;
        private BuildingUnitSO m_UnitSO;
        private Button m_Button;
        private TooltipTrigger m_TooltipTrigger;

        public void Setup(BuildingUnitSO unit)
        {
            m_Button = GetComponent<Button>();
            m_TooltipTrigger = GetComponent<TooltipTrigger>();

            m_Button.onClick.AddListener(Buy);
            m_UnitSO = unit;

            m_Icon.sprite = unit.Icon;

            if (m_TooltipTrigger != null)
                m_TooltipTrigger.OnTriggerEnter += () =>  m_TooltipTrigger.SetTooltip(GenerateDescription(), unit.UnitName);
        }

        private string GenerateDescription()
        {
            string costText = "Cost:\n";
            foreach (CostProps c in m_UnitSO.Cost.CostProps)
            {
                if (ResourceManager.CheckForResource(c.ResourceType, c.Amount))
                    costText += $"-{c.ResourceType}: {c.Amount}\n";
                else
                    costText += $"-<color=red>{c.ResourceType}: {c.Amount}</color>\n";
            }

            string description = $"{m_UnitSO.Description}\n\n";
            description += costText;
            return description;
        }

        public void Buy()
        {
            if (!m_UnitSO.Cost.Check())
            {
                NotificationManager.Notificate("Not enough resources to build!");
                return;
            }
            m_UnitSO.Cost.Spend();
            BuildManager.SetBuildObject(m_UnitSO);
        }
    }
}
