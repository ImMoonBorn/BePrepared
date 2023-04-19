using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.Unit;
using MoonBorn.BePrepared.Utils.SaveSystem;
using System.Collections;
using MoonBorn.UI;

namespace MoonBorn.BePrepared.Gameplay.Consumption
{
    public class ConsumptionManager : Singleton<ConsumptionManager>
    {
        enum Months
        {
            December = 0,
            January = 1,
            Februrary = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11
        }

        enum Seasons
        {
            Winter = 0,
            Spring = 1,
            Summer = 2,
            Autumn = 3
        }

        [Serializable]
        struct ConsumpitonResource
        {
            public ResourceType ResourceType;

            [Header("Consumption")]
            [Range(0.0f, 1.0f)] public float ConsumptionFactor;
            [Range(0.0f, 1.0f)] public float WinterFactor;
            private float ConsumptionAmount;
            private float ConsumptionTotal;
            public float ExtraConsumptions;

            [Header("Morale")]
            [Range(0.0f, 1.0f)] public float GoodEffectFactor;
            [Range(0.0f, 1.0f)] public float BadEffectFactor;

            [Header("UI")]
            public TMP_Text ConsumptionText;

            private float MoraleForumla(int villagerCount, float morale, float effect)
            {
                if (effect <= 0.0f)
                    return 0.0f;

                morale *= 0.01f;
                return 0.05f + (villagerCount * 0.1f) * effect * 10f;
            }

            public float CalculateMorale(int villagerCount, float morale)
            {
                if (villagerCount <= 0)
                    return 0.0f;

                float moraleAmount = 0.0f;

                if (ResourceManager.CheckForResource(ResourceType, ConsumptionTotal))
                    moraleAmount += MoraleForumla(villagerCount, morale, GoodEffectFactor);
                else
                    moraleAmount -= MoraleForumla(villagerCount, morale, BadEffectFactor);

                if (moraleAmount < 0.0f)
                    NotificationManager.Notificate($"{ResourceType} was not engouh.", NotificationType.Warning);

                return moraleAmount;
            }

            public void UseResources()
            {
                ResourceManager.SpendResource(ResourceType, ConsumptionTotal);
            }

            public void CheckResources(Color canAfford, Color cantAfford)
            {
                ConsumptionText.color = ResourceManager.CheckForResource(ResourceType, ConsumptionTotal) ? canAfford : cantAfford;
            }

            public void CalculateConsumption(int villagerCount, bool isWinter)
            {
                ConsumptionAmount = ExtraConsumptions;

                if (villagerCount > 0 && ConsumptionFactor > 0.0f)
                    ConsumptionAmount += (ConsumptionFactor * (villagerCount * villagerCount)) + (villagerCount * (ConsumptionFactor + 1.0f));

                if (isWinter && WinterFactor > 0.0f)
                    ConsumptionTotal = ConsumptionAmount * (WinterFactor + 1.0f);
                else
                    ConsumptionTotal = ConsumptionAmount;

                if (ConsumptionTotal > 0.0f)
                    ConsumptionText.text = $"-{ConsumptionTotal:0.##}";
                else
                    ConsumptionText.text = $"{ConsumptionTotal:0.##}";
            }
        }

        public static float MoraleEfficency => (Instance.m_MoraleAmount - 50.0f) * 0.01f * Instance.m_EfficiencyFactor;

        public static Action OnMonthPassed;

        [Header("Seasons")]
        [SerializeField] private float m_MonthInSeconds = 60.0f;
        [SerializeField] private Image m_MonthTimerImage;
        [SerializeField] private TMP_Text m_MonthText;
        [SerializeField] private TMP_Text m_SeasonText;
        [SerializeField] private TMP_Text m_YearText;
        private Months m_Month = Months.March;
        private Seasons m_Season = Seasons.Spring;
        private float m_MonthTimer = 0.0f;
        private int m_YearIndex = 0;

        [Header("Consumptions")]
        [SerializeField] private ConsumpitonResource m_WoodConsumption;
        [SerializeField] private ConsumpitonResource m_FoodConsumption;
        [SerializeField] private ConsumpitonResource m_StoneConsumption;
        [SerializeField] private Color m_CanAffordColor;
        [SerializeField] private Color m_CantAffordColor;

        [Header("Morale")]
        [SerializeField] private TMP_Text m_MoraleText;
        [SerializeField] private Image m_MoraleIndicator;
        [SerializeField] private Color m_UnhappyColor = Color.red;
        [SerializeField] private Color m_NeutralColor = Color.yellow;
        [SerializeField] private Color m_HappyColor = Color.green;
        private float m_MoraleAmount = 50.0f;

        [Header("Stats")]
        [SerializeField, Range(0.0f, 1.0f)] private float m_EfficiencyFactor = 0.2f;
        [SerializeField] private TMP_Text m_EfficiencyText;

        [Header("Audio")]
        [SerializeField] private AudioSource m_EffectPlayer;
        [SerializeField] private AudioClip m_WinterOverClip;
        [SerializeField] private AudioClip m_WinterWarningClip;

        [Header("UI")]
        [SerializeField] private Image m_WinterNotificator;
        [SerializeField] private TMP_Text m_WinterNotificationText;

        private void Start()
        {
            m_MonthText.text = m_Month.ToString();
            m_YearText.text = $"Year: {m_YearIndex}";
            m_SeasonText.text = GetSeasonFromMonth((int)m_Month).ToString();

            CalculateMorale();
            CalculateConsumptionAmounts();

            UnitManager.Instance.OnVillagerCreated.AddListener(CalculateConsumptionAmounts);
            UnitManager.Instance.OnVillagerDestroyed.AddListener(CalculateConsumptionAmounts);

            ResourceManager.OnResourceChange.AddListener(CheckResources);
        }

        private void Update()
        {
            m_MonthTimer += Time.deltaTime;
            m_MonthTimerImage.fillAmount = m_MonthTimer / m_MonthInSeconds;

            if (m_MonthTimer >= m_MonthInSeconds)
            {
                NotificationManager.Notificate("Month Passed", NotificationType.Success);

                CalculateMorale();
                UseResources();
                NextMonth();
                CalculateConsumptionAmounts();

                m_MonthTimer = 0.0f;

                OnMonthPassed?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Space))
                debug = !debug;
        }

        private void RefreshMoraleUI()
        {
            m_MoraleText.text = $"{(int)m_MoraleAmount}%";

            Color moraleColor;
            if (m_MoraleAmount > 70.0f)
                moraleColor = m_HappyColor;
            else if (m_MoraleAmount > 35.0f)
                moraleColor = m_NeutralColor;
            else
                moraleColor = m_UnhappyColor;

            m_MoraleIndicator.color = moraleColor;

            float efficiency = MoraleEfficency;
            m_EfficiencyText.color = efficiency >= 0.0f ? m_HappyColor : m_UnhappyColor;
            m_EfficiencyText.text = $"{efficiency * 100.0f:0.#}%";
        }

        bool debug = false;

        private void CalculateMorale()
        {
            int villagerCount = UnitManager.VillagerCount;

            float totalMorale = 0.0f;

            totalMorale += m_WoodConsumption.CalculateMorale(villagerCount, m_MoraleAmount);
            if (debug)
                print("Wood: " + m_WoodConsumption.CalculateMorale(villagerCount, m_MoraleAmount));

            totalMorale += m_FoodConsumption.CalculateMorale(villagerCount, m_MoraleAmount);

            if (debug)
                print("Food: " + m_FoodConsumption.CalculateMorale(villagerCount, m_MoraleAmount));

            totalMorale += m_StoneConsumption.CalculateMorale(villagerCount, m_MoraleAmount);

            if (debug)
                print("Stone: " + m_StoneConsumption.CalculateMorale(villagerCount, m_MoraleAmount));


            m_MoraleAmount += totalMorale;
            m_MoraleAmount = Mathf.Clamp(m_MoraleAmount, 0.0f, 100.0f);

            RefreshMoraleUI();
        }

        private void UseResources()
        {
            m_WoodConsumption.UseResources();
            m_FoodConsumption.UseResources();
            m_StoneConsumption.UseResources();
        }

        private void CheckResources()
        {
            m_WoodConsumption.CheckResources(m_CanAffordColor, m_CantAffordColor);
            m_FoodConsumption.CheckResources(m_CanAffordColor, m_CantAffordColor);
            m_StoneConsumption.CheckResources(m_CanAffordColor, m_CantAffordColor);
        }

        private void CalculateConsumptionAmounts()
        {
            int villagerCount = UnitManager.VillagerCount;
            bool isWinter = m_Season == Seasons.Winter;

            m_WoodConsumption.CalculateConsumption(villagerCount, isWinter);
            m_FoodConsumption.CalculateConsumption(villagerCount, isWinter);
            m_StoneConsumption.CalculateConsumption(villagerCount, isWinter);
        }

        private void NextMonth()
        {
            m_Month++;

            if ((int)m_Month > 11)
                m_Month = Months.December;

            if (m_Month == Months.December)
            {
                m_WinterNotificationText.text = "Winter has come.";
                m_EffectPlayer.PlayOneShot(m_WinterWarningClip);
                StartCoroutine(NotificatorAnimation());
            }
            else if (m_Month == Months.March)
            {
                m_WinterNotificationText.text = "Winter is over.";
                m_EffectPlayer.PlayOneShot(m_WinterOverClip);
                StartCoroutine(NotificatorAnimation());
            }
            else if (m_Month == Months.January)
            {
                m_YearIndex++;
                m_YearText.text = $"Year: {m_YearIndex}";
            }

            m_MonthText.text = m_Month.ToString();

            m_Season = GetSeasonFromMonth((int)m_Month);
            m_SeasonText.text = m_Season.ToString();
        }

        private Seasons GetSeasonFromMonth(int month)
        {
            if (month >= 0 && month < 3)
                return Seasons.Winter;
            else if (month >= 3 && month < 6)
                return Seasons.Spring;
            else if (month >= 6 && month < 9)
                return Seasons.Summer;
            else if (month >= 9 && month < 12)
                return Seasons.Autumn;
            else
                return Seasons.Spring;
        }

        private IEnumerator NotificatorAnimation()
        {
            float timer = 0.0f;
            float animationTime = 0.4f;

            Color c = m_WinterNotificator.color;
            c.a = 0.0f;
            m_WinterNotificator.color = c;
            m_WinterNotificationText.alpha = c.a;
            m_WinterNotificator.gameObject.SetActive(true);

            while (timer < animationTime)
            {
                timer += Time.deltaTime;
                c.a = timer / animationTime;
                m_WinterNotificationText.alpha = c.a;
                m_WinterNotificator.color = c;
                yield return null;
            }

            timer = 0.0f;
            float waitTimer = 2.5f;

            while (timer < waitTimer)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            timer = animationTime;

            while (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                c.a = timer / animationTime;
                m_WinterNotificationText.alpha = c.a;
                m_WinterNotificator.color = c;
                yield return null;
            }

            m_WinterNotificator.gameObject.SetActive(false);
        }

        public static void AddConsumptions(ResourceType type, float amount)
        {
            switch (type)
            {
                case ResourceType.Wood: Instance.m_WoodConsumption.ExtraConsumptions += amount; break;
                case ResourceType.Food: Instance.m_FoodConsumption.ExtraConsumptions += amount; break;
                case ResourceType.Stone: Instance.m_StoneConsumption.ExtraConsumptions += amount; break;
                default: break;
            }

            Instance.CalculateConsumptionAmounts();
        }

        public static void RemoveConsumptions(ResourceType type, float amount)
        {
            AddConsumptions(type, -amount);
        }

        private void OnDestroy()
        {
            ResourceManager.OnResourceChange.RemoveListener(CheckResources);
        }

        public static ConsumptionManagerData ResourceManagerData => new ConsumptionManagerData()
        { Month = (int)Instance.m_Month, MonthTimer = Instance.m_MonthTimer, MoraleAmount = Instance.m_MoraleAmount };

        public static void Load(ConsumptionManagerData saveData)
        {
            Instance.m_Month = (Months)saveData.Month;
            Instance.m_MonthTimer = saveData.MonthTimer;
            Instance.m_MoraleAmount = saveData.MoraleAmount;

            Instance.RefreshMoraleUI();
            Instance.CalculateConsumptionAmounts();
        }
    }
}
