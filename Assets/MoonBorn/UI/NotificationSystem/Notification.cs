using System.Collections;
using UnityEngine;
using TMPro;

namespace MoonBorn.UI
{
    public class Notification : MonoBehaviour
    {
        [SerializeField] private RectTransform m_RectTransform;
        [SerializeField] private TMP_Text m_MessageText;
        [SerializeField] private float m_Duration = 5.0f;
        private RectTransform m_Anchor;
        private AudioSource m_AudioSource;

        public void Setup(string message, NotificationType type, RectTransform anchoredPosition, float anchorSize, NotificationAnchorDirection direction)
        {
            if (type != NotificationType.None)
            {
                if (TryGetComponent(out m_AudioSource))
                {
                    if (type == NotificationType.Success)
                    {
                        AudioClip clip = NotificationManager.SuccessClip;
                        if (clip)
                            m_AudioSource.PlayOneShot(clip);
                    }
                    else if(type == NotificationType.Warning)
                    {
                        AudioClip clip = NotificationManager.WarningClip;
                        if (clip)
                            m_AudioSource.PlayOneShot(clip);
                    }
                }
            }

            float offset = FindOffset(direction, anchorSize);

            gameObject.SetActive(true);
            m_RectTransform.anchoredPosition = new Vector3(offset, anchoredPosition.anchoredPosition.y, 0.0f);
            SetAnchor(anchoredPosition);
            m_MessageText.text = message;
            StartCoroutine(DestroyTimer());
        }

        public void SetAnchor(RectTransform anchor)
        {
            m_Anchor = anchor;
        }

        private void Update()
        {
            m_RectTransform.anchoredPosition = Vector3.Lerp(m_RectTransform.anchoredPosition, m_Anchor.anchoredPosition, Time.deltaTime * 8.0f);
        }

        private float FindOffset(NotificationAnchorDirection direction, float anchorSize)
        {
            return direction switch
            {
                NotificationAnchorDirection.Left => -anchorSize,
                NotificationAnchorDirection.Right => anchorSize,
                _ => 0.0f,
            };
        }

        private IEnumerator DestroyTimer()
        {
            yield return new WaitForSeconds(m_Duration);
            NotificationManager.RemoveNotification(this);
            Destroy(gameObject);
        }
    }
}
