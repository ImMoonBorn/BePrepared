using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.UI
{
    public enum NotificationAnchorDirection
    {
        Left, Right
    }

    public enum NotificationType
    {
        None = 0,
        Success = 1,
        Warning = 2,
    }

    public class NotificationManager : Singleton<NotificationManager>
    {
        public static AudioClip SuccessClip => Instance.m_SuccessClip;
        public static AudioClip WarningClip => Instance.m_WarningClip;

        [SerializeField] private Notification m_NotificationPrefab;
        [SerializeField] private RectTransform m_Placeholder;
        [SerializeField] private NotificationAnchorDirection m_AnchorDirection;
        [SerializeField] private RectTransform[] m_Anchors;
        private readonly List<Notification> m_Notifications = new();

        [Header("Audio")]
        [SerializeField] private AudioClip m_SuccessClip;
        [SerializeField] private AudioClip m_WarningClip;

        public static void Notificate(string message, NotificationType type = NotificationType.None)
        {
            Instance.AddNotification(message, type);
        }

        public static void RemoveNotification(Notification notification)
        {
            Instance.m_Notifications.Remove(notification);
        }

        private void AddNotification(string message, NotificationType notificationType = NotificationType.None)
        {
            if (m_Notifications.Count >= m_Anchors.Length)
            {
                int index = m_Notifications.Count - 1;
                Notification n = m_Notifications[index];
                m_Notifications.RemoveAt(index);
                Destroy(n.gameObject);
            }

            for (int i = 0; i < m_Notifications.Count; i++)
            {
                Notification n = m_Notifications[i];
                n.SetAnchor(m_Anchors[i + 1]);
            }

            Notification notification = Instantiate(m_NotificationPrefab, m_Placeholder);
            notification.Setup(message, notificationType, m_Anchors[0], m_Placeholder.sizeDelta.x, m_AnchorDirection);
            m_Notifications.Insert(0, notification);
        }
    }
}