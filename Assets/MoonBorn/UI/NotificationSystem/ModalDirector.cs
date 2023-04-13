using UnityEngine;

namespace MoonBorn.UI
{
    public class ModalDirector : MonoBehaviour
    {
        private RectTransform m_RectTransform;
        [SerializeField] private RectTransform m_ButtonsRect;

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            Vector3 newPosition = m_RectTransform.offsetMin;
            newPosition.x = 0.0f;
            newPosition.y -= 25.0f;

            m_ButtonsRect.anchoredPosition = newPosition;
        }
    }
}
