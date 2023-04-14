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
        [SerializeField] private Image m_BlockMask;
        [SerializeField] private TooltipTrigger m_TooltipTrigger;
        private BuildingAction m_Action;

        public void Setup(BuildingAction action)
        {
            m_Action = action;
            m_Button.onClick.AddListener(action.CallAction);

            m_Icon.sprite = action.Icon;

            if (m_TooltipTrigger != null)
                m_TooltipTrigger.OnTriggerEnter += () => m_TooltipTrigger.SetTooltip(action.GetActionDescription(), action.ActionName);

            m_Action.OnCancelAction += DoBlink;
            m_BlockMask.enabled = false;
        }

        public void Update()
        {
            m_ProgressMask.fillAmount = m_Action.ActionProgress;
        }
       
        private void DoBlink()
        {
            if (m_Action.ReachedLimit)
                StartCoroutine(BlinkWarning());
        }

        private IEnumerator BlinkWarning()
        {
            if (m_BlockMask.enabled)
                yield break;

            m_BlockMask.enabled = true;
            int blinkCount = 3;
            float blinkTimer = 0.1f;
            Color maskColor = m_BlockMask.color;

            for (int i = 0; i < blinkCount; i++)
            {
                float timer = 0.0f;
                maskColor.a = 0.0f;
                m_BlockMask.color = maskColor;

                while (timer < blinkTimer)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                timer = 0.0f;
                maskColor.a = 1.0f;
                m_BlockMask.color = maskColor;

                while (timer < blinkTimer)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            m_BlockMask.enabled = false;
        }

        private void OnDestroy()
        {
            m_Action.OnCancelAction -= DoBlink;
        }

    }
}
