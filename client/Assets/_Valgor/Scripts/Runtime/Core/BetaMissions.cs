using System;
using UnityEngine;

namespace Valgor.Core
{
    /// <summary>
    /// Missões mínimas da Beta 0.2 — progresso + recompensa única, sem campanha complexa.
    /// </summary>
    public static class BetaMissions
    {
        public const string PrefsPrefix = "valgor.missions.v1.";
        public const string KeyChapter = PrefsPrefix + "chapter";
        public const string KeyClaimedMask = PrefsPrefix + "claimed";

        public const int MissionCount = 8;

        public static readonly string[] Titles =
        {
            "O Castelo",
            "Colheita",
            "Pedra sobre pedra",
            "O Rei dos Dragões",
            "Fome do ninho",
            "Além dos muros",
            "Marcha",
            "O espólio"
        };

        public static readonly string[] Objectives =
        {
            "Selecione o Castelo na City.",
            "Colete recursos da Fazenda.",
            "Conclua o upgrade de qualquer edifício.",
            "Abra Heróis e visualize Vortex.",
            "Alimente um dragão na Torre.",
            "Abra o Mapa Mundial.",
            "Envie uma marcha a partir do mapa.",
            "Receba a recompensa de uma marcha."
        };

        public static readonly int[] DiamondRewards = { 2, 3, 5, 4, 5, 3, 6, 8 };

        public static int ActiveChapter
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(KeyChapter, 0), 0, MissionCount);
            private set
            {
                PlayerPrefs.SetInt(KeyChapter, Mathf.Clamp(value, 0, MissionCount));
                PlayerPrefs.Save();
            }
        }

        public static int ClaimedMask
        {
            get => PlayerPrefs.GetInt(KeyClaimedMask, 0);
            private set
            {
                PlayerPrefs.SetInt(KeyClaimedMask, value);
                PlayerPrefs.Save();
            }
        }

        public static bool IsComplete(int index) => ActiveChapter > index;

        public static bool IsClaimed(int index) => (ClaimedMask & (1 << index)) != 0;

        public static bool CanClaim(int index) => IsComplete(index) && !IsClaimed(index);

        public static void Notify(MissionEvent evt)
        {
            if (!LocalPlayerProfile.HasProfile)
            {
                return;
            }

            var expected = ActiveChapter;
            if (expected >= MissionCount)
            {
                return;
            }

            var match = expected switch
            {
                0 => evt == MissionEvent.SelectCastle,
                1 => evt == MissionEvent.CollectFarm,
                2 => evt == MissionEvent.UpgradeComplete,
                3 => evt == MissionEvent.ViewVortex,
                4 => evt == MissionEvent.FeedDragon,
                5 => evt == MissionEvent.OpenWorldMap,
                6 => evt == MissionEvent.SendMarch,
                7 => evt == MissionEvent.ReceiveReward,
                _ => false
            };

            if (!match)
            {
                return;
            }

            ActiveChapter = expected + 1;
            Debug.Log($"[Valgor] Missão concluída: {Titles[expected]}");
        }

        public static bool TryClaim(int index, out int diamonds, out string error)
        {
            diamonds = 0;
            error = string.Empty;
            if (index < 0 || index >= MissionCount)
            {
                error = "Missão inválida.";
                return false;
            }

            if (!CanClaim(index))
            {
                error = IsClaimed(index) ? "Recompensa já recolhida." : "Objetivo ainda incompleto.";
                return false;
            }

            diamonds = DiamondRewards[index];
            ClaimedMask = ClaimedMask | (1 << index);
            return true;
        }

        public static void Wipe()
        {
            PlayerPrefs.DeleteKey(KeyChapter);
            PlayerPrefs.DeleteKey(KeyClaimedMask);
            PlayerPrefs.Save();
        }
    }

    public enum MissionEvent
    {
        SelectCastle,
        CollectFarm,
        UpgradeComplete,
        ViewVortex,
        FeedDragon,
        OpenWorldMap,
        SendMarch,
        ReceiveReward
    }
}
