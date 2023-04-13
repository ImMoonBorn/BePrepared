using System.Collections.Generic;
using UnityEngine;

namespace MoonBorn.Utils
{
    [System.Serializable]
    public enum CellType
    {
        None,
        Tree,
        Stone,
        Avoid,
        Prop,
        Bush
    }

    public class CellComponent : MonoBehaviour
    {
        public Vector2Int Position;
        public CellType CellType;
    }

    public class ProceduralWorldGenerator : MonoBehaviour
    {
        [System.Serializable]
        class CellArrayData
        {
            public string Guid;
            public Vector2Int Position;
            public CellType CellType;
            public int PrefabNumber;
        }

        [System.Serializable]
        class CellArray
        {
            public List<CellArrayData> Cells = new();
        }

        [System.Serializable]
        struct Cell
        {
            public string Guid;
            public CellType Type;
            public int PrefabNumber;
        }

        [System.Serializable]
        struct AvoidMesh
        {
            public Transform AvoidTransform;
            public int CellSize;
        }

        [Header("Components")]
        [SerializeField] private bool m_GenerateGUID = true;
        [SerializeField] private bool m_AddCellComponent = true;

        [Header("Map Settings")]
        [SerializeField] private Vector2Int m_Position = Vector2Int.zero;
        [SerializeField] private Vector2Int m_Size = new Vector2Int(50, 50);
        [SerializeField] private bool m_GenerateTrees = true;
        [SerializeField] private bool m_GenerateStones = true;
        [SerializeField] private bool m_GenerateBushes = true;
        [SerializeField] private bool m_GenerateProps = true;
        [SerializeField] private AvoidMesh[] m_AvoidMeshes;
        private static Cell[,] m_Cells;

        [Header("Trees")]
        [SerializeField] private GameObject[] m_TreePrefabs;
        [SerializeField, Range(0.0f, 1.0f)] private float m_TreeDensity = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float m_TreeNoiseScale = 0.5f;
        [SerializeField] private int m_TreeAvoidanceRadius = 3;

        [Header("Stones")]
        [SerializeField] private GameObject[] m_StonePrefabs;
        [SerializeField, Range(0.0f, 1.0f)] private float m_StoneDensity = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float m_StoneNoiseScale = 0.5f;
        [SerializeField] private int m_StoneAvoidanceRadius = 3;

        [Header("Bushes")]
        [SerializeField] private GameObject[] m_BushPrefabs;
        [SerializeField, Range(0.0f, 1.0f)] private float m_BushDensity = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float m_BushNoiseScale = 0.5f;
        [SerializeField] private int m_BushAvoidanceRadius = 3;

        [Header("Environment")]
        [SerializeField] private GameObject[] m_PropPrefabs;
        [SerializeField, Range(0.0f, 1.0f)] private float m_PropsDensity = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float m_PropsNoiseScale = 0.5f;
        [SerializeField] private int m_PropsAvoidanceRadius = 3;

        private void GenerateAvoids()
        {
            foreach (AvoidMesh mesh in m_AvoidMeshes)
            {
                int x = Mathf.RoundToInt(mesh.AvoidTransform.position.x);
                int y = Mathf.RoundToInt(mesh.AvoidTransform.position.z);
                int radius = mesh.CellSize;

                Vector3 targetPos = new Vector3(x + m_Size.x / 2.0f, 0.0f, y + m_Size.y / 2.0f);
                x = (int)targetPos.x;
                y = (int)targetPos.z;

                for (int dy = y - radius; dy < y + radius; dy++)
                {
                    if (dy >= m_Size.y || dy < 0)
                        continue;

                    for (int dx = x - radius; dx < x + radius; dx++)
                    {
                        if (dx >= m_Size.x || dx < 0)
                            continue;

                        m_Cells[dx, dy].Type = CellType.Avoid;
                    }
                }
            }
        }

        private void GenerateTrees()
        {
            float[,] noiseMap = new float[m_Size.x, m_Size.y];
            float xOffset = Random.Range(-10000.0f, 10000.0f);
            float yOffset = Random.Range(-10000.0f, 10000.0f);

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * m_TreeNoiseScale + xOffset, y * m_TreeNoiseScale + yOffset);
                    noiseMap[x, y] = noiseValue;
                }
            }

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float treeDensity = Random.Range(0.0f, m_TreeDensity);
                    if (noiseMap[x, y] < treeDensity && CheckRadius(x, y, m_TreeAvoidanceRadius))
                        CreateCell(m_TreePrefabs, CellType.Tree, x, y);
                }
            }
        }

        private void GenerateStones()
        {
            float[,] noiseMap = new float[m_Size.x, m_Size.y];
            float xOffset = Random.Range(-10000.0f, 10000.0f);
            float yOffset = Random.Range(-10000.0f, 10000.0f);

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * m_StoneNoiseScale + xOffset, y * m_StoneNoiseScale + yOffset);
                    noiseMap[x, y] = noiseValue;
                }
            }

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float stoneDensity = Random.Range(0.0f, m_StoneDensity);
                    if (noiseMap[x, y] < stoneDensity && CheckRadius(x, y, m_StoneAvoidanceRadius))
                        CreateCell(m_StonePrefabs, CellType.Stone, x, y);
                }
            }
        }

        private void GenerateBushes()
        {
            float[,] noiseMap = new float[m_Size.x, m_Size.y];
            float xOffset = Random.Range(-10000.0f, 10000.0f);
            float yOffset = Random.Range(-10000.0f, 10000.0f);

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * m_BushNoiseScale + xOffset, y * m_BushNoiseScale + yOffset);
                    noiseMap[x, y] = noiseValue;
                }
            }

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float bushDensity = Random.Range(0.0f, m_BushDensity);
                    if (noiseMap[x, y] < bushDensity && CheckRadius(x, y, m_BushAvoidanceRadius))
                        CreateCell(m_BushPrefabs, CellType.Bush, x, y);
                }
            }
        }

        private void GenerateProps()
        {
            float[,] noiseMap = new float[m_Size.x, m_Size.y];
            float xOffset = Random.Range(-10000.0f, 10000.0f);
            float yOffset = Random.Range(-10000.0f, 10000.0f);

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * m_PropsNoiseScale + xOffset, y * m_PropsNoiseScale + yOffset);
                    noiseMap[x, y] = noiseValue;
                }
            }

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    float propDensity = Random.Range(0.0f, m_PropsDensity);
                    if (noiseMap[x, y] < propDensity && CheckRadius(x, y, m_PropsAvoidanceRadius))
                        CreateCell(m_PropPrefabs, CellType.Prop, x, y);
                }
            }
        }

        private void CreateCell(GameObject[] array, CellType type, int x, int y)
        {
            if (array.Length == 0)
                return;

            int prefabNumber = Random.Range(0, array.Length);

            GameObject gObj = Instantiate(array[prefabNumber], GetInstantiatePosition(x, y), Quaternion.identity);

            m_Cells[x, y].Type = type;
            m_Cells[x, y].PrefabNumber = prefabNumber;

            if (m_GenerateGUID)
            {
                GUIDComponent guid;
                if (!gObj.TryGetComponent(out GUIDComponent hasGUID))
                    guid = gObj.AddComponent<GUIDComponent>();
                else
                    guid = hasGUID;

                guid.GenerateGUID();
                m_Cells[x, y].Guid = guid.GUID;
            }
            if (m_AddCellComponent)
            {
                CellComponent cellComponent;
                if (!gObj.TryGetComponent(out CellComponent hasCell))
                    cellComponent = gObj.AddComponent<CellComponent>();
                else
                    cellComponent = hasCell;

                cellComponent.Position = new Vector2Int(x, y);
                cellComponent.CellType = type;
            }
        }

        public static void DestroyCell(int x, int y)
        {
            m_Cells[x, y].Guid = string.Empty;
            m_Cells[x, y].Type = CellType.None;
            m_Cells[x, y].PrefabNumber = 0;
        }

        private bool CheckRadius(int x, int y, int radius, CellType type = CellType.None)
        {
            bool canBuild = true;

            for (int dy = y - radius; dy < y + radius; dy++)
            {
                if (dy >= m_Size.y || dy < 0)
                    continue;

                for (int dx = x - radius; dx < x + radius; dx++)
                {
                    if (dx >= m_Size.x || dx < 0)
                        continue;

                    if (m_Cells[dx, dy].Type != type)
                    {
                        canBuild = false;
                        break;
                    }
                }
            }
            return canBuild;
        }

        private Vector3 GetInstantiatePosition(int x, int y)
        {
            return new Vector3(x - (m_Size.x / 2.0f) + m_Position.x, 0.0f, y - (m_Size.y / 2.0f) + m_Position.y);
        }

        private GameObject GetGameobject(CellType type, int x, int y, int prefabNumber)
        {
            return type switch
            {
                CellType.Tree => Instantiate(m_TreePrefabs[prefabNumber], GetInstantiatePosition(x, y), Quaternion.identity),
                CellType.Stone => Instantiate(m_StonePrefabs[prefabNumber], GetInstantiatePosition(x, y), Quaternion.identity),
                CellType.Bush => Instantiate(m_BushPrefabs[prefabNumber], GetInstantiatePosition(x, y), Quaternion.identity),
                CellType.Prop => Instantiate(m_PropPrefabs[prefabNumber], GetInstantiatePosition(x, y), Quaternion.identity),
                _ => null
            };
        }

        [ContextMenu("Generate")]
        public void GenerateWorld()
        {
            if (!Application.isPlaying)
                return;

            m_Cells = new Cell[m_Size.x, m_Size.y];

            GenerateAvoids();
            if (m_GenerateTrees && m_TreePrefabs.Length > 0)
                GenerateTrees();
            if (m_GenerateStones && m_StonePrefabs.Length > 0)
                GenerateStones();
            if (m_GenerateBushes && m_BushPrefabs.Length > 0)
                GenerateBushes();
            if (m_GenerateProps && m_PropPrefabs.Length > 0)
                GenerateProps();
        }

        [ContextMenu("Save")]
        public void SaveWorld()
        {
            if (!Application.isPlaying)
                return;

            CellArray array = new CellArray();

            for (int y = 0; y < m_Size.y; y++)
            {
                for (int x = 0; x < m_Size.x; x++)
                {
                    if (m_Cells[x, y].Type == CellType.Avoid || m_Cells[x, y].Type == CellType.None)
                        continue;

                    var cellData = new CellArrayData()
                    {
                        Guid = m_Cells[x, y].Guid,
                        Position = new Vector2Int(x, y),
                        PrefabNumber = m_Cells[x, y].PrefabNumber,
                        CellType = m_Cells[x, y].Type
                    };
                    array.Cells.Add(cellData);
                }
            }
            FileManager.Save($"{Application.persistentDataPath}/WorldData.txt", array);
        }

        [ContextMenu("Load")]
        public void LoadWorld()
        {
            if (!Application.isPlaying)
                return;

            m_Cells = new Cell[m_Size.x, m_Size.y];

            CellArray array = FileManager.Load<CellArray>($"{Application.persistentDataPath}/WorldData.txt");

            foreach (var cell in array.Cells)
            {
                int x = cell.Position.x;
                int y = cell.Position.y;

                int prefabNumber = cell.PrefabNumber;

                GameObject gObj = GetGameobject(cell.CellType, x, y, prefabNumber);

                m_Cells[x, y].Type = cell.CellType;
                m_Cells[x, y].PrefabNumber = prefabNumber;

                if (m_GenerateGUID && gObj != null)
                {
                    GUIDComponent guid;
                    if (!gObj.TryGetComponent(out GUIDComponent hasGUID))
                        guid = gObj.AddComponent<GUIDComponent>();
                    else
                        guid = hasGUID;

                    guid.SetGuid(cell.Guid);
                    m_Cells[x, y].Guid = guid.GUID;
                }

                if (m_AddCellComponent)
                {
                    CellComponent cellComponent;
                    if (!gObj.TryGetComponent(out CellComponent hasCell))
                        cellComponent = gObj.AddComponent<CellComponent>();
                    else
                        cellComponent = hasCell;

                    cellComponent.Position = cell.Position;
                    cellComponent.CellType = cell.CellType;
                }
            }
        }
    }
}
