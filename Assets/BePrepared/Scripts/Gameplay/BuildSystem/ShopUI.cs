using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoonBorn.BePrepared.Gameplay.BuildSystem
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private RectTransform m_ShopRect;
        [SerializeField] private Button m_ActivateButton;
        [SerializeField] private GameObject m_ActiveIcon;
        [SerializeField] private GameObject m_InactiveIcon;
        [SerializeField] private float m_DelayTime = 0.3f;
        private bool m_ShouldClose = false;
        private Vector3 m_StartPosition = Vector3.zero;

        private void Start()
        {
            m_StartPosition = m_ShopRect.anchoredPosition;

            OpenShop();
            m_ActivateButton.onClick.AddListener(HandleShopState);
        }

        private void HandleShopState()
        {
            m_ShouldClose = !m_ShouldClose;
            if (m_ShouldClose)
                CloseShop();
            else
                OpenShop();
        }

        private void OpenShop()
        {
            m_InactiveIcon.SetActive(false);
            m_ActiveIcon.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(MoveShop());
        }

        private void CloseShop()
        {
            m_InactiveIcon.SetActive(true);
            m_ActiveIcon.SetActive(false);

            StopAllCoroutines();
            StartCoroutine(MoveShop());
        }

        private IEnumerator MoveShop()
        {
            float timer = 0.0f;

            while (timer <= m_DelayTime)
            {
                if (m_ShouldClose)
                {
                    Vector3 newPosition = m_ShopRect.anchoredPosition;
                    newPosition.x = -m_ShopRect.sizeDelta.x;
                    m_ShopRect.anchoredPosition = Vector3.Lerp(m_ShopRect.anchoredPosition, newPosition, timer / m_DelayTime);
                }
                else
                    m_ShopRect.anchoredPosition = Vector3.Lerp(m_ShopRect.anchoredPosition, m_StartPosition, timer / m_DelayTime);
                
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}
