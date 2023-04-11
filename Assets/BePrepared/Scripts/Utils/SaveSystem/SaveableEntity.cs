using UnityEngine;
using MoonBorn.Utils;
using System.Collections.Generic;

namespace MoonBorn.BePrepared.Utils.SaveSystem
{
    public class SaveableEntity : MonoBehaviour
    {
        private GUIDComponent m_Guid;

        private void Start()
        {
            m_Guid = GetComponent<GUIDComponent>();
        }

        public void SaveState()
        {
            foreach (var saveable in GetComponents<ISaveable>())
                saveable.SaveState(m_Guid.GUID);
        }

        public void LoadState(Dictionary<string, SaveData> saveValues)
        {
            if(saveValues.TryGetValue(m_Guid.GUID, out SaveData save))
                foreach (var saveable in GetComponents<ISaveable>())
                    saveable.LoadState(save);
        }
    }
}
