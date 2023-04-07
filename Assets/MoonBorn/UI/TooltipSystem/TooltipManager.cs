using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoonBorn.Utils;

namespace MoonBorn.UI
{
    public class TooltipManager : Singleton<TooltipManager>
    {
        [SerializeField] private float m_Delay = 0.65f;
        [SerializeField] private float m_FadeTime = 0.2f;
        [SerializeField] private Tooltip m_Tooltip;
        [SerializeField] private Image m_TooltipBackground;

        private void Awake()
        {
            Hide();
        }

        public static void Show(string content, string header = "")
        {
            Instance.StartCoroutine(Instance.ShowTooltip(content, header));
        }

        public static void Hide()
        {
            Instance.StopAllCoroutines();
            Instance.m_Tooltip.gameObject.SetActive(false);

            Color color = Instance.m_TooltipBackground.color;
            Color newColor = color;
            newColor.a = 0.0f;
            Instance.m_TooltipBackground.color = newColor;
            Instance.m_Tooltip.SetText("");
        }

        private IEnumerator ShowTooltip(string content, string header = "")
        {
            float timer = 0.0f;
            while (timer < m_Delay)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            m_Tooltip.gameObject.SetActive(true);
            m_Tooltip.SetText(content, header);

            timer = 0.0f;
            m_TooltipBackground.fillAmount = 0.0f;
            Color color = m_TooltipBackground.color;
            Color newColor = color;
            newColor.a = 0.0f;
            m_TooltipBackground.color = newColor;

            while (timer < m_FadeTime)
            {
                newColor.a = timer / m_FadeTime;
                m_TooltipBackground.color = newColor;
                timer += Time.deltaTime;
                yield return null;
            }

        }
    }
}
