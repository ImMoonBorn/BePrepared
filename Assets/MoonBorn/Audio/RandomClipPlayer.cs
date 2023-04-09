using UnityEngine;

namespace MoonBorn.Audio
{
    public class RandomClipPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] m_Clips;
        [SerializeField] private AudioSource m_AudioSource;

        public void SetClips(AudioClip[] clips)
        {
            m_Clips = clips;
        }

        public void PlayOneShot()
        {
            if (m_Clips.Length > 0 && m_AudioSource)
                m_AudioSource.PlayOneShot(m_Clips[Random.Range(0, m_Clips.Length)]);
        }
    }
}
