using System.Collections;
using UnityEngine;

namespace MoonBorn.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource m_MusicSource;
        [SerializeField] private AudioClip[] m_Musics;
        private int m_Index = 0;

        private void Start()
        {
            StartPlay();
        }

        public void StartPlay()
        {
            m_MusicSource.clip = m_Musics[m_Index];
            m_MusicSource.Play();
            StartCoroutine(WaitToEndAudio());
        }

        private IEnumerator WaitToEndAudio()
        {
            yield return new WaitForSeconds(m_Musics[m_Index].length);
            m_Index++;
            if(m_Index >= m_Musics.Length)
                m_Index = 0;

            StartPlay();
        }
        
    }
}
