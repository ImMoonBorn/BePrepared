using UnityEngine;
using UnityEngine.SceneManagement;
using MoonBorn.BePrepared.Utils.SaveSystem;
using MoonBorn.Utils;

namespace MoonBorn.BePrepared.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        private static bool s_LoadGame;

        [SerializeField] private ProceduralWorldGenerator m_ProceduralWorldGenerator;

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
            SaveManager.Save();
        }

        [ContextMenu("Load Game")]
        public void LoadGame()
        {
            s_LoadGame = true;
            SceneManager.LoadScene(0);
        }
    }
}
