using UnityEngine;
using MoonBorn.BePrepared.Gameplay;

namespace MoonBorn.BePrepared.Utils
{
    public class CursorTextureChanger : MonoBehaviour
    {
        [SerializeField] private Texture2D m_Texture;
        [SerializeField] private Vector2 m_Offset = new Vector2(5.0f, 2.0f);

        public void ChangeCursor() => GameManager.SetCursorTexture(m_Texture, m_Offset);
    }
}

