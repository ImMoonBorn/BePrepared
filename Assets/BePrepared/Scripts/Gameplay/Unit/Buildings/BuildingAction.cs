using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using MoonBorn.UI;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public abstract class BuildingAction : MonoBehaviour
    {
        public UnityAction OnCallAction;
        public UnityAction OnFinishAction;
        public UnityAction OnCancelAction;

        public float ActionProgress => m_NormalizedTimer;
        public bool InAction => m_DoAction;
        public string ActionName => m_ActionName;
        public bool ReachedLimit => m_ReachedLimit;
        public bool DestroyOnLimitReach => m_DestroyOnLimitReach;
        public Sprite Icon => m_Icon;
        public bool BlockAction => !m_DoAction && (m_ReachedLimit || !CustomCondition() || !m_Cost.Check());

        [Header("Settings")]
        [SerializeField] protected float m_ActionTime = 5.0f;
        [SerializeField] private ResourceCost m_Cost;
        [SerializeField] protected bool m_DestroyOnLimitReach = false;
        protected bool m_DoAction = false;
        protected float m_ActionTimer = 0.0f;
        protected bool m_ReachedLimit = false;
        private float m_NormalizedTimer = 0.0f;

        [Header("Visual")]
        [SerializeField] private string m_ActionName;
        [SerializeField] private string m_ActionDescription;
        [SerializeField] private Sprite m_Icon;

        protected abstract void OnActionFinish();
        protected abstract void OnActionCall();
        protected abstract void OnActionCancel();
        protected abstract bool CustomCondition();
        protected abstract void OnActionUpdate();

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

        public void ContinueAction(float actionTime)
        {
            m_ActionTimer = actionTime;
            m_DoAction = true;
            StartCoroutine(DoAction());
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
                    OnActionCall();
                    return;
                }
                else
                    m_ReachedLimit = false;

                OnCallAction?.Invoke();
                OnActionCall();
                m_ActionTimer = 0.0f;

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
                m_ActionTimer = 0.0f;

                OnActionCancel();
                OnCancelAction?.Invoke();
            }
        }

        protected IEnumerator DoAction()
        {
            while (m_ActionTimer < m_ActionTime)
            {
                m_NormalizedTimer = m_ActionTimer / m_ActionTime;
                m_ActionTimer += Time.deltaTime;
                OnActionUpdate();
                yield return null;
            }

            OnActionFinish();
            OnFinishAction?.Invoke();
            m_DoAction = false;
            m_NormalizedTimer = 0.0f;
            m_ActionTimer = 0.0f;
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
