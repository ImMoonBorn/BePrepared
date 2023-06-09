using System;
using UnityEngine;

namespace MoonBorn.BePrepared.Gameplay.Unit
{
    public class UnitImprovements : MonoBehaviour
    {
        public static Action OnVillagerImprovement;

        public static float LumberjackSpeed => m_LumberjackSpeed;
        public static float FarmerSpeed => m_FarmerSpeed;
        public static float MinerSpeed => m_MinerSpeed;

        private static float m_LumberjackSpeed = 0.0f;
        private static float m_FarmerSpeed = 0.0f;
        private static float m_MinerSpeed = 0.0f;

        private void Awake()
        {
            m_LumberjackSpeed = 0.0f;
            m_FarmerSpeed = 0.0f;
            m_MinerSpeed = 0.0f;
        }

        public static void SetLumberjackSpeed(float value)
        {
            m_LumberjackSpeed = value;
            OnVillagerImprovement?.Invoke();
        }

        public static void SetFarmerSpeed(float value)
        {
            m_FarmerSpeed = value;
            OnVillagerImprovement?.Invoke();
        }

        public static void SetMinerSpeed(float value)
        {
            m_MinerSpeed = value;
            OnVillagerImprovement?.Invoke();
        }
    }
}
