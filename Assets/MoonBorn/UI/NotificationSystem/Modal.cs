using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoonBorn.Utils;

namespace MoonBorn.UI
{
    public class Modal : Singleton<Modal>
    {
        [SerializeField] private GameObject m_ConfirmBox;
        [SerializeField] private Button m_ConfirmButton;
        [SerializeField] private Button m_CancelButton;
        private Action m_OnConfirmAction;

        [SerializeField] private TMP_Text m_HeaderText;
        [SerializeField] private TMP_Text m_ContentText;
        [SerializeField] private LayoutElement m_LayoutElement;
        [SerializeField] private int m_WrapLimit;

        private void Awake()
        {
            m_ConfirmBox.SetActive(false);

            m_ConfirmButton.onClick.AddListener(OnConfirm);
            m_CancelButton.onClick.AddListener(OnCancel);
        }

        public static void OpenModal(Action action, string content, string header = "")
        {
            Instance.m_ConfirmBox.SetActive(true);
            Instance.SetText(content, header);
            Instance.m_OnConfirmAction = action;
        }

        private void SetText(string content, string header = "")
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

        private void OnConfirm()
        {
            m_OnConfirmAction?.Invoke();
            Instance.m_ConfirmBox.SetActive(false);
            m_OnConfirmAction = null;
        }

        private void OnCancel()
        {
            Instance.m_ConfirmBox.SetActive(false);
            m_OnConfirmAction = null;
        }
    }
}
