using System;
using UnityEngine;

namespace MoonBorn.Utils
{
    public class GUIDComponent : MonoBehaviour
    {
        public string GUID => m_GUID;

        [SerializeField] private string m_GUID = string.Empty;

        [ContextMenu("Generate Id")]
        public void GenerateGUID() => m_GUID = Guid.NewGuid().ToString();

        public void SetGuid(string guid)
        { 
            m_GUID = guid; 
        }

        public static string CreateGUID() => Guid.NewGuid().ToString();
    }
}
