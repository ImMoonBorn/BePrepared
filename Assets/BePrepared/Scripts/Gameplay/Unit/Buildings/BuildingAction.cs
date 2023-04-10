using MoonBorn.UI;
using System.Collections;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public abstract class BuildingAction : MonoBehaviour
    {
        public float ActionProgress => m_NormalizedTimer;
        public bool InAction => m_DoAction;
        public string ActionName => m_ActionName;

        public Sprite Icon => m_Icon;

        [Header("Settings")]
        [SerializeField] protected float m_ActionTime = 5.0f;
        [SerializeField] private ResourceCost m_Cost;
        protected bool m_DoAction = false;
        private float m_NormalizedTimer = 0.0f;

        [Header("Visual")]
        [SerializeField] private string m_ActionName;
        [SerializeField] private string m_ActionDescription;
        [SerializeField] private Sprite m_Icon;

        protected abstract void OnAction();
        protected abstract bool CustomCondition();

        protected abstract string CustomConditionWarningMessage();

        public void CallAction()
        {
            m_DoAction = !m_DoAction;

            if (m_DoAction)
            {
                if (!CustomCondition())
                {
                    NotificationManager.Notificate(CustomConditionWarningMessage(), NotificationType.Warning);
                    m_DoAction = false;
                    return;
                }

                if (m_Cost.Check())
                {
                    StartCoroutine(DoAction());
                    m_Cost.Spend();
                }
                else
                {
                    NotificationManager.Notificate("Not Enough Resources!", NotificationType.Warning);
                    m_DoAction = false;
                }
            }
            else
            {
                m_Cost.Restore();
                StopAllCoroutines();
                m_NormalizedTimer = 0.0f;
            }
        }

        protected IEnumerator DoAction()
        {
            float actionTimer = 0.0f;

            while (actionTimer < m_ActionTime)
            {
                m_NormalizedTimer = actionTimer / m_ActionTime;
                actionTimer += Time.deltaTime;
                yield return null;
            }

            OnAction();
            m_DoAction = false;
            m_NormalizedTimer = 0.0f;
        }

        public string GetActionDescription()
        {
            CostProps[] costs = m_Cost.CostProps;
            string costText = "Cost:\n";
            foreach (CostProps c in costs)
            {
                if (ResourceManager.CheckForResource(c.ResourceType, c.Amount))
                    costText += $"-{c.ResourceType}: {c.Amount}\n";
                else
                    costText += $"-<color=red>{c.ResourceType}: {c.Amount}</color>\n";
            }

            return $"{m_ActionDescription}\n\n{costText}";
        }
    }
}
