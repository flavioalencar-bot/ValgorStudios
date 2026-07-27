using System;
using UnityEngine;

namespace Valgor.Core
{
    /// <summary>
    /// Progressão transversal da beta (castelo + pesquisa leve).
    /// </summary>
    public static class BetaProgress
    {
        public const string KeyCastleLevel = LocalPlayerProfile.PrefsPrefix + "castleLevel";
        public const string KeyResearchGather = LocalPlayerProfile.PrefsPrefix + "research.gatherBoost";

        public static int CastleLevel
        {
            get => Mathf.Max(1, PlayerPrefs.GetInt(KeyCastleLevel, 1));
            set
            {
                PlayerPrefs.SetInt(KeyCastleLevel, Mathf.Clamp(value, 1, 99));
                PlayerPrefs.Save();
            }
        }

        public static bool ResearchGatherBoost
        {
            get => PlayerPrefs.GetInt(KeyResearchGather, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyResearchGather, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void SyncCastleLevel(int level) =>
            CastleLevel = Math.Max(CastleLevel, Mathf.Clamp(level, 1, 99));

        public static void UnlockGatherResearch() => ResearchGatherBoost = true;

        public static void Wipe()
        {
            PlayerPrefs.DeleteKey(KeyCastleLevel);
            PlayerPrefs.DeleteKey(KeyResearchGather);
        }

        public static string Describe() =>
            $"Castelo Nv.{CastleLevel}" +
            (ResearchGatherBoost ? " · Pesquisa: Coleta +" : string.Empty);
    }
}
