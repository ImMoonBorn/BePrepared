using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.UI
{
    public enum NotificationAnchorDirection
    {
        Left, Right
    }

    public class NotificationManager : Singleton<NotificationManager>
    {
        [SerializeField] private Notification m_NotificationPrefab;
        [SerializeField] private RectTransform m_Placeholder;
        [SerializeField] private NotificationAnchorDirection m_AnchorDirection;
        [SerializeField] private RectTransform[] m_Anchors;
        private readonly List<Notification> m_Notifications = new();

        public static void Notificate(string message)
        {
            Instance.AddNotification(message);
        }

        public static void RemoveNotification(Notification notification)
        {
            Instance.m_Notifications.Remove(notification);
        }

        private void AddNotification(string message)
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
            notification.Setup(m_Anchors[0], message, m_AnchorDirection, m_Placeholder.sizeDelta.x);
            m_Notifications.Insert(0, notification);
        }
    }
}