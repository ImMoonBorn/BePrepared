using UnityEngine;
using UnityEngine.SceneManagement;
using MoonBorn.BePrepared.Utils.SaveSystem;
using MoonBorn.Utils;
using UnityEngine.UI;
using MoonBorn.UI;

namespace MoonBorn.BePrepared.Gameplay
{
    public enum GameState
    {
        Playing,
        Paused
    }

    public class GameManager : Singleton<GameManager>
    {
        public static GameState GameState => Instance.m_Gamestate;

        private static bool s_LoadGame;

        [SerializeField] private ProceduralWorldGenerator m_ProceduralWorldGenerator;
        private GameState m_Gamestate = GameState.Playing;

        [Header("UI")]
        [SerializeField] private GameObject m_PauseMenuCanvas;
        [SerializeField] private Button m_ResumeGameButton;
        [SerializeField] private Button m_NewGameButton;
        [SerializeField] private Button m_SaveGameButton;
        [SerializeField] private Button m_LoadGameButton;
        [SerializeField] private Button m_QuitGameButton;

        private void Awake()
        {
            m_ResumeGameButton.onClick.AddListener(PlayGame);
            m_NewGameButton.onClick.AddListener(NewGame);
            m_SaveGameButton.onClick.AddListener(SaveGame);
            m_LoadGameButton.onClick.AddListener(LoadGame);
            m_QuitGameButton.onClick.AddListener(() => Application.Quit());

            PlayGame();
        }

        private void Start()
        {
            if (s_LoadGame)
            {
                m_ProceduralWorldGenerator.LoadWorld();
                SaveManager.Load();
            }
            else
                m_ProceduralWorldGenerator.GenerateWorld();
        }

        [ContextMenu("New Game")]
        public void NewGame()
        {
            s_LoadGame = false;
            SceneManager.LoadScene(0);
        }

        [ContextMenu("Save Game")]
        public void SaveGame()
        {
            m_ProceduralWorldGenerator.SaveWorld();
            SaveManager.Save();
            PlayGame();
            NotificationManager.Notificate("Game Saved.", NotificationType.Success);
        }

        [ContextMenu("Load Game")]
        public void LoadGame()
        {
            s_LoadGame = true;
            SceneManager.LoadScene(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (m_Gamestate == GameState.Playing)
                    PauseGame();
                else if (m_Gamestate == GameState.Paused)
                    PlayGame();
            }
        }

        private void PlayGame()
        {
            m_Gamestate = GameState.Playing;
            Time.timeScale = 1.0f;
            m_PauseMenuCanvas.SetActive(false);
        }

        private void PauseGame()
        {
            m_Gamestate = GameState.Paused;
            Time.timeScale = 0.0f;
            m_PauseMenuCanvas.SetActive(true);
        }
    }
}
