using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoonBorn.UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string m_Header;
        [Multiline()]
        [SerializeField] private string m_Content;

        public Action OnTriggerEnter;
        public Action OnTriggerExit;

        public void SetTooltip(string content, string header)
        {
            m_Content = content;
            m_Header = header;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnTriggerEnter?.Invoke();
            TooltipManager.Show(m_Content, m_Header);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnTriggerExit?.Invoke();
            TooltipManager.Hide();
        }
    }
}
