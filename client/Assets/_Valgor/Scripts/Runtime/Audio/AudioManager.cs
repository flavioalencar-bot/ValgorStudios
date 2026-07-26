using System.Collections.Generic;
using UnityEngine;
using Valgor.Pooling;

namespace Valgor.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        private const string MusicVolumeKey = "valgor.audio.music";
        private const string SfxVolumeKey = "valgor.audio.sfx";
        private readonly Queue<AudioSource> availableSfxSources = new();
        private AudioSource musicSource;
        private float musicVolume;
        private float sfxVolume;

        public float MusicVolume
        {
            get => musicVolume;
            set { musicVolume = Mathf.Clamp01(value); musicSource.volume = musicVolume; PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume); }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set { sfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume); }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            musicSource.volume = musicVolume;
        }

        public void PlayMusic(AudioClip clip, bool restart = false)
        {
            if (clip == null || (!restart && musicSource.clip == clip && musicSource.isPlaying)) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            var source = GetSfxSource();
            source.clip = clip;
            source.volume = sfxVolume * Mathf.Clamp01(volumeScale);
            source.Play();
            StartCoroutine(ReturnAfterPlayback(source));
        }

        private AudioSource GetSfxSource()
        {
            if (availableSfxSources.Count > 0) return availableSfxSources.Dequeue();
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private System.Collections.IEnumerator ReturnAfterPlayback(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            source.clip = null;
            availableSfxSources.Enqueue(source);
        }
    }
}
