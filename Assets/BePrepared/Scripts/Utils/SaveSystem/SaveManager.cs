using System;
using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.BePrepared.Utils.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public string GUID;
        public Vector3 Position;
        public Vector3 Rotation;
    }

    [Serializable]
    public class ResourceData : SaveData
    {
        public int ResourceAmount;
    }

    [Serializable]
    public class SaveDatas
    {
        public List<SaveData> RawData = new();
        public List<ResourceData> ResourceDAta = new();
    }

    public class SaveManager : Singleton<SaveManager>
    {
        private static string SavePath => $"{Application.persistentDataPath}/SaveData.txt";

        private static SaveDatas m_SaveDatas = new();

        [SerializeField] private ProceduralWorldGenerator m_WorldGenerator;

        public static void SaveToRawData(SaveData saveData)
        {
            m_SaveDatas.RawData.Add(saveData);
        }

        public static void SaveToTreeData(ResourceData resourceData)
        {
            m_SaveDatas.ResourceDAta.Add(resourceData);
        }

        public static void Save()
        {
            m_SaveDatas = new();
            SaveAlLDatas();
            FileManager.Save(SavePath, m_SaveDatas);
            Instance.m_WorldGenerator.SaveData();
        }

        public static void Load()
        {
            m_SaveDatas = FileManager.Load<SaveDatas>(SavePath);
            LoadAllDatas(m_SaveDatas);
        }

        private static void SaveAlLDatas()
        {
            foreach (var saveable in FindObjectsOfType<SaveableEntity>())
                saveable.SaveState();
        }

        private static void LoadAllDatas(SaveDatas saveDatas)
        {
            Dictionary<string, SaveData> saveVals= new Dictionary<string, SaveData>();

            foreach(var s in saveDatas.RawData)
                saveVals.Add(s.GUID, s);
            foreach(var t in saveDatas.ResourceDAta)
                saveVals.Add(t.GUID, t);

            foreach (var saveable in FindObjectsOfType<SaveableEntity>())
                saveable.LoadState(saveVals);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
                Save();
            else if (Input.GetKeyDown(KeyCode.L))
                Load();
        }
    }
}

