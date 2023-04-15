using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoonBorn.UI;
using MoonBorn.Utils;
using MoonBorn.BePrepared.Gameplay.Unit;
using MoonBorn.BePrepared.Utils.SaveSystem;

namespace MoonBorn.BePrepared.Gameplay
{
    public enum GameState
    {
        Playing,
        Paused
    }

    public enum MouseState
    {
        Idle,
        Building,
        Menu
    }

    public class GameManager : Singleton<GameManager>
    {
        private static string FolderPath => $"{Application.persistentDataPath}/SaveFolder";
        private static string WorldDataPath => $"{FolderPath}/WorldData.txt";
        private static string SaveDataPath => $"{FolderPath}/SaveData.txt";

        public static GameState GameState => Instance.m_Gamestate;
        public static MouseState MouseState => Instance.m_MouseState;

        private static bool s_LoadGame;

        [SerializeField] private bool m_IsMainMenu = false;
        [SerializeField] private Texture2D m_CursorTexture;
        [SerializeField] private ProceduralWorldGenerator m_ProceduralWorldGenerator;
        private GameState m_Gamestate = GameState.Playing;
        private MouseState m_MouseState = MouseState.Idle;
        private MouseState m_LastMouseState = MouseState.Idle;

        [Header("Game Rules")]
        [SerializeField] private Transform m_SpawnTransform;
        [SerializeField] private int m_PopulationLimit = 5;
        [SerializeField] private int m_StartingVillagers = 3;
        [SerializeField] private ResourceCost m_StartingResources;

        [Header("UI")]
        [SerializeField] private GameObject m_PauseMenuCanvas;
        [SerializeField] private Button m_ResumeGameButton;
        [SerializeField] private Button m_NewGameButton;
        [SerializeField] private Button m_SaveGameButton;
        [SerializeField] private Button m_LoadGameButton;
        [SerializeField] private Button m_QuitGameButton;
        [SerializeField] private Button m_MainMenuButton;

        private void Awake()
        {
            if (m_ResumeGameButton)
                m_ResumeGameButton.onClick.AddListener(ResumeGame);

            if (m_NewGameButton)
                m_NewGameButton.onClick.AddListener(NewGame);

            if (m_SaveGameButton)
                m_SaveGameButton.onClick.AddListener(SaveGame);

            if (m_LoadGameButton)
            {
                bool canLoad = true;

                if (Directory.Exists(FolderPath))
                {
                    if (!File.Exists(SaveDataPath))
                        canLoad = false;
                    if (!File.Exists(WorldDataPath))
                        canLoad = false;
                }
                else
                    canLoad = false;

                m_LoadGameButton.interactable = canLoad;
                m_LoadGameButton.onClick.AddListener(LoadGame);
            }

            if (m_QuitGameButton)
                m_QuitGameButton.onClick.AddListener(() => Application.Quit());

            if (m_MainMenuButton)
                m_MainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));

            ResumeGame();

            SetCursorTexture(m_CursorTexture, new Vector2(10.0f, 4.0f));
        }

        private void Start()
        {
            if (m_IsMainMenu)
                return;

            if (s_LoadGame)
            {
                UnitManager.IncreasePopulationLimit(m_PopulationLimit);
                m_ProceduralWorldGenerator.LoadWorld(WorldDataPath);
                SaveManager.Load(SaveDataPath);
            }
            else
            {
                m_StartingResources.Restore();
                m_ProceduralWorldGenerator.GenerateWorld();
                UnitManager.IncreasePopulationLimit(m_PopulationLimit);

                m_StartingVillagers = Mathf.Min(m_StartingVillagers, m_PopulationLimit);
                int distance = 2;
                int x = 0, y = m_StartingVillagers * distance;
                for (int i = 0; i < m_StartingVillagers; i++)
                {
                    if (x % 3 == 0)
                    {
                        x = 0;
                        y -= distance;
                    }
                    x += distance;

                    Vector3 spawnPos = m_SpawnTransform.position;
                    Vector3 position = new Vector3(x, 0.0f, y) + spawnPos;
                    UnitManager.CreateVillager(position, Vector3.zero, false);
                }
            }
        }

        [ContextMenu("New Game")]
        public void NewGame()
        {
            s_LoadGame = false;
            SceneManager.LoadScene(1);
        }

        [ContextMenu("Save Game")]
        public void SaveGame()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            m_ProceduralWorldGenerator.SaveWorld(WorldDataPath);
            SaveManager.Save(SaveDataPath);
            ResumeGame();
            NotificationManager.Notificate("Game Saved.", NotificationType.Success);
        }

        [ContextMenu("Load Game")]
        public void LoadGame()
        {
            s_LoadGame = true;
            SceneManager.LoadScene(1);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !m_IsMainMenu)
            {
                if (m_Gamestate == GameState.Playing)
                    PauseGame();
                else if (m_Gamestate == GameState.Paused)
                    ResumeGame();
            }
        }

        private void ResumeGame()
        {
            if (m_IsMainMenu)
                return;

            m_Gamestate = GameState.Playing;
            Time.timeScale = 1.0f;
            m_PauseMenuCanvas.SetActive(false);

            RestoreMouseState();
        }

        private void PauseGame()
        {
            if (m_IsMainMenu)
                return;

            m_Gamestate = GameState.Paused;
            Time.timeScale = 0.0f;
            m_PauseMenuCanvas.SetActive(true);

            SetMouseState(MouseState.Menu);
        }

        public static void SetMouseState(MouseState mouseState)
        {
            Instance.m_LastMouseState = Instance.m_MouseState;
            Instance.m_MouseState = mouseState;
        }

        public static void RestoreMouseState()
        {
            Instance.m_MouseState = Instance.m_LastMouseState;
        }

        public static void SetCursorTexture(Texture2D texture, Vector2 offset)
        {
            Cursor.SetCursor(texture, offset, CursorMode.Auto);
        }

        public static void ResetCursorTexture()
        {
            SetCursorTexture(Instance.m_CursorTexture, new Vector2(10.0f, 4.0f));
        }
    }
}
