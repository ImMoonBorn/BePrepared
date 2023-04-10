using UnityEngine;

namespace MoonBorn.Utils
{
    public class ProceduralWorldGenerator : MonoBehaviour
    {
        enum CellType
        {
            None,
            Tree,
            Stone,
            Avoid
        }

        struct Cell
        {
            public CellType Type;
        }

        [System.Serializable]
        struct AvoidMesh
        {
            public Transform AvoidTransform;
            public int CellSize;
        }

        [Header("Map Settings")]
        [SerializeField] private Vector2Int m_Position = Vector2Int.zero;
        [SerializeField] private Vector2Int m_Size = new Vector2Int(50, 50);
        [SerializeField] private bool m_GenerateTrees = true;
        [SerializeField] private bool m_GenerateStones = true;
        [SerializeField] private bool m_GenerateProps = true;
        [SerializeField] private AvoidMesh[] m_AvoidMeshes;
        private Cell[,] m_Cells;

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

        [Header("Environment")]
        [SerializeField] private GameObject[] m_PropPrefabs;
        [SerializeField, Range(0.0f, 1.0f)] private float m_PropsDensity = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float m_PropsNoiseScale = 0.5f;
        [SerializeField] private int m_PropsAvoidanceRadius = 3;

        private void Start()
        {
            m_Cells = new Cell[m_Size.x, m_Size.y];

            GenerateAvoids();
            if (m_GenerateTrees && m_TreePrefabs.Length > 0)
                GenerateTrees();
            if (m_GenerateStones && m_StonePrefabs.Length > 0)
                GenerateStones();
            if (m_GenerateProps && m_PropPrefabs.Length > 0)
                GenerateProps();

            Destroy(this);
        }

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
                    {
                        Instantiate(m_TreePrefabs[Random.Range(0, m_TreePrefabs.Length)], GetInstantiatePosition(x, y), Quaternion.identity);
                        m_Cells[x, y].Type = CellType.Tree;
                    }
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
                    {
                        Instantiate(m_StonePrefabs[Random.Range(0, m_StonePrefabs.Length)], GetInstantiatePosition(x, y), Quaternion.identity);
                        m_Cells[x, y].Type = CellType.Stone;
                    }
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
                    float stoneDensity = Random.Range(0.0f, m_PropsDensity);
                    if (noiseMap[x, y] < stoneDensity && CheckRadius(x, y, m_PropsAvoidanceRadius))
                    {
                        Instantiate(m_PropPrefabs[Random.Range(0, m_PropPrefabs.Length)], GetInstantiatePosition(x, y), Quaternion.identity);
                        m_Cells[x, y].Type = CellType.Avoid;
                    }
                }
            }
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
    }
}
