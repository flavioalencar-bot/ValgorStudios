using System;
using System.Text;
using UnityEngine;

namespace Valgor.Core
{
    /// <summary>
    /// Diagnóstico de save apenas em log (nunca na UI do jogador).
    /// Fonte do Player: PlayerPrefs do executável (HKCU\Software\Valgor Studios\Valgor no Windows).
    /// O Editor usa store separado — não é fonte do Valgor.exe.
    /// </summary>
    public static class SaveDiagnostics
    {
        public const string SaveSchemaVersion = "player.v1";

        public static void LogSnapshot(string reason)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Valgor.Save] --- diagnóstico ---");
            sb.AppendLine($"reason={reason}");
            sb.AppendLine($"schema={SaveSchemaVersion}");
            sb.AppendLine($"bundle={ValgorVersion.Bundle}");
            sb.AppendLine($"display={ValgorVersion.Display}");
#if UNITY_EDITOR
            sb.AppendLine("store=UnityEditor (NÃO é o save do Valgor.exe)");
#else
            sb.AppendLine("store=Player (executável)");
#endif
            sb.AppendLine($"company={Application.companyName}");
            sb.AppendLine($"product={Application.productName}");
            sb.AppendLine($"hasProfile={LocalPlayerProfile.HasProfile}");
            sb.AppendLine($"playerId={(LocalPlayerProfile.HasProfile ? LocalPlayerProfile.PlayerId : "-")}");
            sb.AppendLine($"playerName={(LocalPlayerProfile.HasProfile ? LocalPlayerProfile.DisplayName : "-")}");
            sb.AppendLine($"introDone={LocalPlayerProfile.IntroDone}");
            sb.AppendLine($"tutorialStep={LocalPlayerProfile.TutorialStep}");
            sb.AppendLine($"lastScene={LocalPlayerProfile.LastScene}");
            sb.AppendLine($"canContinue={LocalPlayerProfile.CanContinue()}");
            sb.AppendLine($"hasDomainSave={LocalPlayerProfile.HasDomainSave()}");
            sb.AppendLine($"missionsChapter={BetaMissions.ActiveChapter}");
            sb.AppendLine($"createdUtc={PlayerPrefs.GetString(LocalPlayerProfile.KeyCreatedUtc, "-")}");
            sb.AppendLine($"loggedAtUtc={DateTime.UtcNow:O}");
            sb.AppendLine("[Valgor.Save] --- fim ---");
            Debug.Log(sb.ToString());
        }
    }
}
