using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.Unit;
using UnityEngine.Rendering;

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
                return (villagerCount * villagerCount * 0.01f) * ((1.0f - morale) * (morale * effect * 100.0f)) + 0.75f;
            }

            public float CalculateMorale(int villagerCount, float morale)
            {
                if (villagerCount <= 0)
                    return 0.0f;

                float moraleAmount = 0.0f;

                if (ResourceManager.CheckForResource(ResourceType, ConsumptionTotal))
                    moraleAmount += MoraleForumla(villagerCount, morale, GoodEffectFactor);
                else
                    moraleAmount -= MoraleForumla(villagerCount, morale, BadEffectFactor * 2.0f);

                return moraleAmount;
            }

            public void UseResources()
            {
                ResourceManager.SpendResource(ResourceType, ConsumptionTotal);
            }

            public void CalculateConsumption(int villagerCount, bool isWinter)
            {
                ConsumptionAmount = (ConsumptionFactor * (villagerCount * villagerCount)) + (villagerCount * (ConsumptionFactor + 1.0f));

                if (isWinter)
                    ConsumptionTotal = ConsumptionAmount * (WinterFactor + 1.0f);
                else
                    ConsumptionTotal = ConsumptionAmount;

                ConsumptionTotal += ExtraConsumptions;
                ConsumptionText.text = $"-{ConsumptionTotal}";
            }
        }

        public static float MoraleEfficency => (Instance.m_MoraleAmount - 50.0f) * 0.01f * Instance.m_EfficiencyFactor;

        public static Action OnMonthPassed;

        [Header("Seasons")]
        [SerializeField] private float m_MonthInSeconds = 60.0f;
        [SerializeField] private Image m_MonthTimerImage;
        [SerializeField] private TMP_Text m_MonthText;
        [SerializeField] private TMP_Text m_SeasonText;
        private Months m_Month = Months.March;
        private Seasons m_Season = Seasons.Spring;
        private float m_MonthTimer = 0.0f;

        [Header("Consumptions")]
        [SerializeField] private ConsumpitonResource m_WoodConsumption;
        [SerializeField] private ConsumpitonResource m_FoodConsumption;
        [SerializeField] private ConsumpitonResource m_StoneConsumption;

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

        private void Start()
        {
            m_MonthText.text = m_Month.ToString();
            m_SeasonText.text = GetSeasonFromMonth((int)m_Month).ToString();

            CalculateMorale();
            CalculateConsumptionAmounts();

            UnitManager.Instance.OnVillagerCreated.AddListener(CalculateConsumptionAmounts);
            UnitManager.Instance.OnVillagerDestroyed.AddListener(CalculateConsumptionAmounts);
        }

        private void Update()
        {
            m_MonthTimer += Time.deltaTime;
            m_MonthTimerImage.fillAmount = m_MonthTimer / m_MonthInSeconds;

            if (m_MonthTimer >= m_MonthInSeconds)
            {
                CalculateMorale();
                UseResources();
                NextMonth();
                CalculateConsumptionAmounts();

                m_MonthTimer = 0.0f;

                OnMonthPassed?.Invoke();
            }
        }

        private void CalculateMorale()
        {
            int villagerCount = UnitManager.VillagerCount;

            float totalMorale = 0.0f;

            totalMorale += m_WoodConsumption.CalculateMorale(villagerCount, m_MoraleAmount);
            totalMorale += m_FoodConsumption.CalculateMorale(villagerCount, m_MoraleAmount);
            totalMorale += m_StoneConsumption.CalculateMorale(villagerCount, m_MoraleAmount);

            m_MoraleAmount += totalMorale;
            m_MoraleAmount = Mathf.RoundToInt(m_MoraleAmount);
            m_MoraleAmount = Mathf.Clamp(m_MoraleAmount, 0.0f, 100.0f);

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
            m_EfficiencyText.text = $"{efficiency * 100.0f}%";
        }

        private void UseResources()
        {
            m_WoodConsumption.UseResources();
            m_FoodConsumption.UseResources();
            m_StoneConsumption.UseResources();
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
    }
}
