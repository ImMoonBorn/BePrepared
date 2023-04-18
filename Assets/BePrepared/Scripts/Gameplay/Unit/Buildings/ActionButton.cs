using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoonBorn.UI;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class ActionButton : MonoBehaviour
    {
        [SerializeField] private Button m_Button;
        [SerializeField] private Image m_Icon;
        [SerializeField] private Image m_ProgressMask;
        [SerializeField] private TooltipTrigger m_TooltipTrigger;
        private BuildingAction m_Action;

        public void Setup(BuildingAction action)
        {
            m_Action = action;
            m_Button.onClick.AddListener(action.CallAction);

            m_Icon.sprite = action.Icon;

            if (m_TooltipTrigger != null)
                m_TooltipTrigger.OnTriggerEnter += () => m_TooltipTrigger.SetTooltip(action.GetActionDescription(), action.ActionName);

            RefreshButton();
            ResourceManager.OnResourceChange.AddListener(RefreshButton);
        }

        public void Update()
        {
            m_ProgressMask.fillAmount = m_Action.ActionProgress;
        }

        private void RefreshButton()
        {
            m_Button.interactable = !m_Action.BlockAction;
        }

        private void OnDestroy()
        {
            ResourceManager.OnResourceChange.RemoveListener(RefreshButton);
        }

    }
}
