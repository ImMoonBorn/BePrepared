using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.Unit;
using MoonBorn.BePrepared.Gameplay.BuildSystem;
using MoonBorn.BePrepared.Gameplay.Consumption;

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
        public int Gender;
    }

    [Serializable]
    public class ResourceManagerData
    {
        public float Wood;
        public float Food;
        public float Stone;
    }

    [Serializable]
    public class ImprovementData
    {
        public string ImprovementName;
        public bool IsDone;
        public float Progress;
        public bool IsImproving;
        public string BuildingGUID;
    }

    [Serializable]
    public class ConsumptionManagerData
    {
        public int Month;
        public float MonthTimer;
        public float MoraleAmount;
    }

    [Serializable]
    public class SaveDatas
    {
        public ResourceManagerData ResourceManagerData = new();
        public ConsumptionManagerData ConsumptionManagerData = new();
        public List<ImprovementData> ImprovementDatas = new();
        public List<SaveData> RawData = new();
        public List<ConstructionData> ConstructionDatas = new();
        public List<BuildingData> BuildingDatas = new();
        public List<VillagerData> VillagerDatas = new();
        public List<ResourceData> ResourceDatas = new();
    }

    public class SaveManager : Singleton<SaveManager>
    {
        private static SaveDatas s_SaveDatas = new();
        private static Dictionary<string, SaveableEntity> s_Entites = new();

        private void Awake()
        {
            s_SaveDatas = new();
            s_Entites = new();
        }

        public static void SaveToRawData(SaveData saveData)
        {
            s_SaveDatas.RawData.Add(saveData);
        }

        public static void SaveToResourceData(ResourceData resourceData)
        {
            s_SaveDatas.ResourceDatas.Add(resourceData);
        }

        public static void SaveToVillagerData(VillagerData villagerData)
        {
            s_SaveDatas.VillagerDatas.Add(villagerData);
        }

        public static void SaveToConstructionData(ConstructionData constructionData)
        {
            s_SaveDatas.ConstructionDatas.Add(constructionData);
        }

        public static void SaveToBuildingData(BuildingData buildingData)
        {
            s_SaveDatas.BuildingDatas.Add(buildingData);
        }

        public static void Save(string path)
        {
            s_SaveDatas = new();
            SaveAlLDatas();
            FileManager.Save(path, s_SaveDatas);
        }

        public static void Load(string path)
        {
            s_SaveDatas = FileManager.Load<SaveDatas>(path);
            LoadAllDatas(s_SaveDatas);
        }

        private static void SaveAlLDatas()
        {
            s_SaveDatas.ResourceManagerData = ResourceManager.ResourceManagerData;
            s_SaveDatas.ConsumptionManagerData = ConsumptionManager.ResourceManagerData;

            var imporvements = ImprovementManager.Improvements;
            foreach (var improvement in imporvements)
            {
                s_SaveDatas.ImprovementDatas.Add
                (
                    new ImprovementData
                    {
                        ImprovementName = improvement.ImprovementSO.Name,
                        IsDone = improvement.IsDone,
                        BuildingGUID = improvement.BuildingGUID,
                        Progress = improvement.Progress,
                        IsImproving = improvement.IsImproving
                    }
                );
            }

            foreach (var saveable in FindObjectsOfType<SaveableEntity>())
                saveable.SaveState();
        }

        private static void LoadAllDatas(SaveDatas saveDatas)
        {
            Dictionary<string, SaveData> saveVals = new Dictionary<string, SaveData>();

            ResourceManager.SetResourceManagerData(s_SaveDatas.ResourceManagerData);
            
            foreach (ImprovementData id in saveDatas.ImprovementDatas)
            {
                ImprovementProp prop = ImprovementManager.FindImprovement(id.ImprovementName);
                if (prop != null && id.IsDone)
                {
                    prop.Improve();
                    continue;
                }
                prop.SetBuildingGUID(id.BuildingGUID);
                prop.SetProgress(id.Progress);
                prop.SetImproving(id.IsImproving);
            }

            foreach (SaveData s in saveDatas.RawData)
                saveVals.Add(s.GUID, s);

            foreach (ResourceData rd in saveDatas.ResourceDatas)
                saveVals.Add(rd.GUID, rd);

            foreach (ConstructionData cd in saveDatas.ConstructionDatas)
            {
                saveVals.Add(cd.GUID, cd);

                UnitBuildingSO unitSO = BuildManager.GetBuildingByName(cd.ConstractionUnitName);
                UnitConstruction construction = Instantiate(unitSO.ConstructionPrefab, cd.Position, Quaternion.Euler(cd.Rotation));

                if (construction.TryGetComponent(out GUIDComponent guid))
                    guid.SetGuid(cd.GUID);

                construction.Setup(unitSO);
            }

            foreach (BuildingData bd in saveDatas.BuildingDatas)
            {
                saveVals.Add(bd.GUID, bd);

                UnitBuildingSO unitSO = BuildManager.GetBuildingByName(bd.BuildingUnitName);
                GameObject building = Instantiate(unitSO.FinishedPrefab, bd.Position, Quaternion.Euler(bd.Rotation));

                building.GetComponent<UnitBuilding>().SetResources(bd.HasResoures);

                if (building.TryGetComponent(out GUIDComponent guid))
                    guid.SetGuid(bd.GUID);
            }

            foreach (VillagerData v in saveDatas.VillagerDatas)
            {
                UnitVillager villager = UnitManager.CreateVillager(v.Position, v.Rotation, false, (VillagerGender)v.Gender);
                if (villager.TryGetComponent(out GUIDComponent entity))
                {
                    entity.SetGuid(v.GUID);
                    saveVals.Add(v.GUID, v);
                }
                else
                    villager.GetComponent<UnitMember>().DestroyUnit();
            }

            ConsumptionManager.Load(s_SaveDatas.ConsumptionManagerData);

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
    }
}

