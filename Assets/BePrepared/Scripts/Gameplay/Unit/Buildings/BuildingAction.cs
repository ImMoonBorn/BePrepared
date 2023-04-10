using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using MoonBorn.UI;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public abstract class BuildingAction : MonoBehaviour
    {
        public UnityAction OnCallAction;
        public UnityAction OnActionFinished;

        public float ActionProgress => m_NormalizedTimer;
        public bool InAction => m_DoAction;
        public string ActionName => m_ActionName;
        public bool ReachedLimit => m_ReachedLimit;
        public bool DestroyOnLimitReach => m_DestroyOnLimitReach;
        public Sprite Icon => m_Icon;

        [Header("Settings")]
        [SerializeField] protected float m_ActionTime = 5.0f;
        [SerializeField] private ResourceCost m_Cost;
        [SerializeField] protected bool m_DestroyOnLimitReach = false;
        protected bool m_DoAction = false;
        private float m_NormalizedTimer = 0.0f;
        protected bool m_ReachedLimit = false;

        [Header("Visual")]
        [SerializeField] private string m_ActionName;
        [SerializeField] private string m_ActionDescription;
        [SerializeField] private Sprite m_Icon;

        protected abstract void OnAction();
        protected abstract bool CustomCondition();

        protected abstract string CustomConditionWarningMessage();

        protected abstract string CustomDescription();

        protected abstract string CustomFooter();

        public abstract void Refresh();

        protected void OverrideAction(string name, float actionTime, ResourceCost cost, Sprite Icon)
        {
            m_ActionName = name;
            m_Cost = cost;
            m_ActionTime = actionTime;
            m_Icon = Icon;
        }

        public void CallAction()
        {
            m_DoAction = !m_DoAction;

            if (m_DoAction)
            {
                if (!CustomCondition())
                {
                    NotificationManager.Notificate(CustomConditionWarningMessage(), NotificationType.Warning);
                    m_DoAction = false;
                    m_ReachedLimit = true;
                    OnCallAction?.Invoke();
                    return;
                }
                else
                    m_ReachedLimit = false;

                OnCallAction?.Invoke();

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
            OnActionFinished?.Invoke();
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

            string finalText = "";
            if (!string.IsNullOrEmpty(m_ActionDescription))
                finalText += m_ActionDescription + " ";

            string customDesc = CustomDescription();
            if (!string.IsNullOrEmpty(customDesc))
                finalText += customDesc;

            if (!string.IsNullOrEmpty(costText))
                finalText += $"\n\n{costText}";

            string customFooter = CustomFooter();
            if (!string.IsNullOrEmpty(customFooter))
                finalText += $"\n{customFooter}";

            return finalText;
        }
    }
}
