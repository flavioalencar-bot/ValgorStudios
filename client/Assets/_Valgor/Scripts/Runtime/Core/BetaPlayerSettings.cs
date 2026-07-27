using UnityEngine;

namespace Valgor.Core
{
    /// <summary>
    /// Preferências de jogador persistidas (áudio/gráficos mínimos da Beta 0.1).
    /// </summary>
    public static class BetaPlayerSettings
    {
        private const string Prefix = "valgor.settings.v1.";
        private const string KeyMasterVolume = Prefix + "masterVolume";
        private const string KeyMusicEnabled = Prefix + "musicEnabled";
        private const string KeySfxEnabled = Prefix + "sfxEnabled";

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(KeyMasterVolume, 0.85f);
            set
            {
                PlayerPrefs.SetFloat(KeyMasterVolume, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                AudioListener.volume = MasterVolume;
            }
        }

        public static bool MusicEnabled
        {
            get => PlayerPrefs.GetInt(KeyMusicEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyMusicEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool SfxEnabled
        {
            get => PlayerPrefs.GetInt(KeySfxEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KeySfxEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void ApplyRuntime()
        {
            AudioListener.volume = MasterVolume;
        }
    }
}
