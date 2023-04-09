using System.Collections;
using UnityEngine;
using MoonBorn.Utils;

namespace MoonBorn.Audio
{
    [System.Serializable]
    public class Playlist
    {
        public string PlaylistName => m_PlaylistName;
        public float GetLenght => Musics[CurrentIndex].length;
        public AudioClip GetClip => Musics[CurrentIndex];

        [SerializeField] private string m_PlaylistName = "Playlist";
        public AudioClip[] Musics;
        public int CurrentIndex;

        public void NextClip()
        {
            CurrentIndex++;
            if (CurrentIndex >= Musics.Length)
                CurrentIndex = 0;
        }
    }

    public class MusicPlayer : Singleton<MusicPlayer>
    {
        private Playlist GetPlaylist => m_Playlists[m_PlaylistIndex];

        [SerializeField] private AudioSource m_MusicSource;
        [SerializeField] private Playlist[] m_Playlists;
        private int m_PlaylistIndex = 0;

        private void Start()
        {
            StartPlay();
        }

        public static void ChangePlaylist(int index, bool resetIndex = true)
        {
            Instance.ChangePlaylistThenPlay(index, resetIndex);
        }

        private void ChangePlaylistThenPlay(int index, bool resetIndex = true)
        {
            m_PlaylistIndex = index;
            if (resetIndex)
                m_Playlists[index].CurrentIndex = 0;
            StartPlay();
        }

        public void StartPlay()
        {
            m_MusicSource.clip = GetPlaylist.GetClip;
            m_MusicSource.Play();
            StartCoroutine(WaitToEndAudio());
        }

        private IEnumerator WaitToEndAudio()
        {
            Playlist playlist = GetPlaylist;
            yield return new WaitForSeconds(playlist.GetLenght);
            playlist.NextClip();
            StartPlay();
        }

    }
}
