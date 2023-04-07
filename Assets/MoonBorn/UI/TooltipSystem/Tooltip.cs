using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonBorn.UI
{
    public class Tooltip : MonoBehaviour
    {
        private RectTransform m_RectTransform;

        [SerializeField] private TMP_Text m_HeaderText;
        [SerializeField] private TMP_Text m_ContentText;
        [SerializeField] private LayoutElement m_LayoutElement;
        [SerializeField] private int m_WrapLimit;

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            Vector2 position = Input.mousePosition;

            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            m_RectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        public void SetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
                m_HeaderText.gameObject.SetActive(false);
            else
            {
                m_HeaderText.gameObject.SetActive(true);
                m_HeaderText.text = header;
            }

            m_ContentText.text = content;

            int headerLength = m_HeaderText.text.Length;
            int contentLenght = m_ContentText.text.Length;

            m_LayoutElement.enabled = (headerLength > m_WrapLimit || contentLenght > m_WrapLimit);
        }
    }
}
