using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Valgor.Core
{
    /// <summary>
    /// Perfil local mínimo da Beta Técnica 0.1 (PlayerPrefs).
    /// </summary>
    public static class LocalPlayerProfile
    {
        public const string PrefsPrefix = "valgor.player.v1.";
        public const string KeyId = PrefsPrefix + "id";
        public const string KeyName = PrefsPrefix + "name";
        public const string KeyCreatedUtc = PrefsPrefix + "createdUtc";
        public const string KeyIntroDone = PrefsPrefix + "introDone";
        public const string KeyTutorialStep = PrefsPrefix + "tutorialStep";
        public const string KeyLastScene = PrefsPrefix + "lastScene";

        private static readonly Regex NamePattern = new(
            @"^[\p{L}\p{N} ]{3,20}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool HasProfile =>
            PlayerPrefs.HasKey(KeyId) && !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(KeyName, string.Empty));

        public static string PlayerId => PlayerPrefs.GetString(KeyId, string.Empty);
        public static string DisplayName => PlayerPrefs.GetString(KeyName, string.Empty);
        public static bool IntroDone => PlayerPrefs.GetInt(KeyIntroDone, 0) == 1;

        public static int TutorialStep
        {
            get => PlayerPrefs.GetInt(KeyTutorialStep, 0);
            set
            {
                PlayerPrefs.SetInt(KeyTutorialStep, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static bool TutorialComplete => TutorialStep >= TutorialSteps.Complete;

        public static string LastScene
        {
            get => PlayerPrefs.GetString(KeyLastScene, SceneIds.City);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                PlayerPrefs.SetString(KeyLastScene, value.Trim());
                PlayerPrefs.Save();
            }
        }

        public static bool TryValidateName(string raw, out string normalized, out string error)
        {
            normalized = (raw ?? string.Empty).Trim();
            if (normalized.Length < 3 || normalized.Length > 20)
            {
                error = "O nome deve ter entre 3 e 20 caracteres.";
                return false;
            }

            if (!NamePattern.IsMatch(normalized))
            {
                error = "Use apenas letras, números e espaços.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool Create(string displayName, out string error)
        {
            if (!TryValidateName(displayName, out var normalized, out error))
            {
                return false;
            }

            WipeDomainSaves();
            var id = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(KeyId, id);
            PlayerPrefs.SetString(KeyName, normalized);
            PlayerPrefs.SetString(KeyCreatedUtc, DateTime.UtcNow.ToString("O"));
            PlayerPrefs.SetInt(KeyIntroDone, 0);
            PlayerPrefs.SetInt(KeyTutorialStep, TutorialSteps.SelectCastle);
            PlayerPrefs.SetString(KeyLastScene, SceneIds.City);
            BetaProgress.CastleLevel = 1;
            SeedStartingEnergy();
            PlayerPrefs.Save();
            error = string.Empty;
            return true;
        }

        /// <summary>Energia inicial 100/100 para a Beta 0.1.</summary>
        public static void SeedStartingEnergy()
        {
            const string energy = "valgor.worldmap.energy.v1";
            PlayerPrefs.SetInt(energy + ".current", 100);
            PlayerPrefs.SetInt(energy + ".max", 100);
        }

        public static void MarkIntroDone()
        {
            PlayerPrefs.SetInt(KeyIntroDone, 1);
            PlayerPrefs.Save();
        }

        public static void AdvanceTutorial()
        {
            if (TutorialComplete)
            {
                return;
            }

            TutorialStep = TutorialStep + 1;
        }

        public static void AdvanceTutorialTo(int minimumStep)
        {
            if (TutorialComplete)
            {
                return;
            }

            if (TutorialStep < minimumStep)
            {
                TutorialStep = minimumStep;
            }
        }

        public static void ApplyToSession(GameSession session)
        {
            if (session == null || !HasProfile)
            {
                return;
            }

            if (!session.IsActive)
            {
                session.Begin();
            }

            session.Authenticate(PlayerId, DisplayName);
        }

        public static bool HasDomainSave()
        {
            return PlayerPrefs.HasKey("valgor.dragons.v3.meta") ||
                   PlayerPrefs.HasKey("valgor.city.production.v1.meta") ||
                   PlayerPrefs.HasKey("valgor.worldmap.v1.meta") ||
                   PlayerPrefs.HasKey("valgor.worldmap.energy.v1.current") ||
                   PlayerPrefs.HasKey(KeyLastScene);
        }

        public static bool CanContinue() => HasProfile && (HasDomainSave() || IntroDone);

        public static void WipeAllForNewJourney()
        {
            WipeDomainSaves();
            PlayerPrefs.DeleteKey(KeyId);
            PlayerPrefs.DeleteKey(KeyName);
            PlayerPrefs.DeleteKey(KeyCreatedUtc);
            PlayerPrefs.DeleteKey(KeyIntroDone);
            PlayerPrefs.DeleteKey(KeyTutorialStep);
            PlayerPrefs.DeleteKey(KeyLastScene);
            PlayerPrefs.Save();
        }

        public static void WipeDomainSaves()
        {
            PlayerPrefs.DeleteKey("valgor.city.production.v1.meta");
            foreach (var id in CityBuildingIds)
            {
                PlayerPrefs.DeleteKey("valgor.city.production.v1.slot." + id + ".lv");
                PlayerPrefs.DeleteKey("valgor.city.production.v1.slot." + id + ".st");
                PlayerPrefs.DeleteKey("valgor.city.production.v1.slot." + id + ".up");
                PlayerPrefs.DeleteKey("valgor.city.production.v1.b." + id + ".acc");
                PlayerPrefs.DeleteKey("valgor.city.production.v1.b." + id + ".ts");
            }

            PlayerPrefs.DeleteKey("valgor.dragons.v3.meta");
            PlayerPrefs.DeleteKey("valgor.worldmap.v1.meta");
            PlayerPrefs.DeleteKey("valgor.worldmap.energy.v1.current");
            PlayerPrefs.DeleteKey("valgor.worldmap.camera.v1.saved");
            PlayerPrefs.DeleteKey("valgor.worldmap.filters.v1.cities");
            BetaProgress.Wipe();
            PlayerPrefs.Save();
        }

        private static readonly string[] CityBuildingIds =
        {
            "castle", "farm", "lumbermill", "quarry", "mine", "warehouse", "academy",
            "institute", "hospital", "market", "temple", "dragon-tower", "arena", "laboratory",
            "wall"
        };

        /// <summary>Passos do tutorial mínimo da Beta 0.1.</summary>
        public static class TutorialSteps
        {
            public const int SelectCastle = 0;
            public const int SelectFarm = 1;
            public const int OpenHeroes = 2;
            public const int ViewVortex = 3;
            public const int OpenDragons = 4;
            public const int FeedDragon = 5;
            public const int OpenMap = 6;
            public const int SelectResource = 7;
            public const int SendMarch = 8;
            public const int ReceiveReward = 9;
            public const int ReturnCity = 10;
            public const int Complete = 11;
        }
    }
}
