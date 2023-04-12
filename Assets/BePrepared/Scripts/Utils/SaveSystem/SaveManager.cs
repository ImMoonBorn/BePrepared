using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.Unit;
using MoonBorn.BePrepared.Gameplay.BuildSystem;

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
        public float ResourceAmount;
    }

    [Serializable]
    public class ConstructionData : SaveData
    {
        public string ConstractionUnitName;
        public float BuildedAmount;
    }

    [Serializable]
    public class BuildingData : SaveData
    {
        public string BuildingUnitName;
        public string[] HasResoures;
    }

    [Serializable]
    public class VillagerData : SaveData
    {
        public string AssignedResourceGUID;
        public string AssignedConstructionGUID;
        public float GatherTimer;
    }

    [Serializable]
    public class SaveDatas
    {
        public List<SaveData> RawData = new();
        public List<ConstructionData> ConstructionData = new();
        public List<BuildingData> BuildingData = new();
        public List<VillagerData> VillagerData = new();
        public List<ResourceData> ResourceData = new();
    }

    public class SaveManager : Singleton<SaveManager>
    {
        private static string s_SavePath => $"{Application.persistentDataPath}/SaveData.txt";

        private static SaveDatas s_SaveDatas = new();
        private static Dictionary<string, SaveableEntity> s_Entites = new();

        [SerializeField] private ProceduralWorldGenerator m_WorldGenerator;

        public static void SaveToRawData(SaveData saveData)
        {
            s_SaveDatas.RawData.Add(saveData);
        }

        public static void SaveToResourceData(ResourceData resourceData)
        {
            s_SaveDatas.ResourceData.Add(resourceData);
        }

        public static void SaveToVillagerData(VillagerData villagerData)
        {
            s_SaveDatas.VillagerData.Add(villagerData);
        }

        public static void SaveToConstructionData(ConstructionData constructionData)
        {
            s_SaveDatas.ConstructionData.Add(constructionData);
        }

        public static void SaveToBuildingData(BuildingData buildingData)
        {
            s_SaveDatas.BuildingData.Add(buildingData);
        }

        public static void Save()
        {
            s_SaveDatas = new();
            SaveAlLDatas();
            FileManager.Save(s_SavePath, s_SaveDatas);
            Instance.m_WorldGenerator.SaveData();
        }

        public static void Load()
        {
            s_SaveDatas = FileManager.Load<SaveDatas>(s_SavePath);
            LoadAllDatas(s_SaveDatas);
        }

        private static void SaveAlLDatas()
        {
            foreach (var saveable in FindObjectsOfType<SaveableEntity>())
                saveable.SaveState();
        }

        private static void LoadAllDatas(SaveDatas saveDatas)
        {
            Dictionary<string, SaveData> saveVals = new Dictionary<string, SaveData>();

            foreach (SaveData s in saveDatas.RawData)
                saveVals.Add(s.GUID, s);

            foreach (ResourceData rd in saveDatas.ResourceData)
                saveVals.Add(rd.GUID, rd);

            foreach (ConstructionData cd in saveDatas.ConstructionData)
            {
                saveVals.Add(cd.GUID, cd);

                UnitBuildingSO unitSO = BuildManager.GetBuildingByName(cd.ConstractionUnitName);
                UnitConstruction construction = Instantiate(unitSO.ConstructionPrefab, cd.Position, Quaternion.Euler(cd.Rotation));

                if (construction.TryGetComponent(out GUIDComponent guid))
                    guid.SetGuid(cd.GUID);

                construction.Setup(unitSO);
            }

            foreach (BuildingData bd in saveDatas.BuildingData)
            {
                saveVals.Add(bd.GUID, bd);

                UnitBuildingSO unitSO = BuildManager.GetBuildingByName(bd.BuildingUnitName);
                GameObject building = Instantiate(unitSO.FinishedPrefab, bd.Position, Quaternion.Euler(bd.Rotation));

                building.GetComponent<UnitBuilding>().SetResources(bd.HasResoures);

                if (building.TryGetComponent(out GUIDComponent guid))
                    guid.SetGuid(bd.GUID);
            }

            var villagers = UnitManager.IdleVillagers;
            foreach (var villager in villagers)
                villager.GetComponent<UnitMember>().DestroyUnit();

            foreach (VillagerData v in saveDatas.VillagerData)
            {
                UnitVillager villager = UnitManager.CreateVillager(v.Position, v.Rotation);
                if (villager.TryGetComponent(out GUIDComponent entity))
                {
                    entity.SetGuid(v.GUID);
                    saveVals.Add(v.GUID, v);
                }
                else
                    villager.GetComponent<UnitMember>().DestroyUnit();
            }

            Instance.StartCoroutine(WaitToLoad(saveVals));
        }

        private static IEnumerator WaitToLoad(Dictionary<string, SaveData> saveVals)
        {
            yield return new WaitForEndOfFrame();

            var list = FindObjectsOfType<SaveableEntity>();

            for (int i = 0; i < list.Length; i++)
            {
                var entity = list[i];
                s_Entites.Add(entity.GUID, entity);
            }

            for (int i = 0; i < list.Length; i++)
            {
                var saveable = list[i];
                saveable.LoadState(saveVals);
            }
        }

        public static bool TryFindFromGUID(string guid, out SaveableEntity entity)
        {
            if (s_Entites.TryGetValue(guid, out SaveableEntity entt))
            {
                entity = entt;
                return true;
            }
            entity = null;
            return false;
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

