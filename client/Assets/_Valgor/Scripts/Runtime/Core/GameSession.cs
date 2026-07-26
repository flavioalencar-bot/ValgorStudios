using System;

namespace Valgor.Core
{
    /// <summary>
    /// Estado de sessão do jogador no cliente. Não contém regras de heróis.
    /// </summary>
    public sealed class GameSession
    {
        public Guid SessionId { get; private set; }
        public DateTime StartedAtUtc { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string AccessToken { get; private set; } = string.Empty;
        public string PlayerDisplayName { get; private set; } = string.Empty;

        public void Begin()
        {
            SessionId = Guid.NewGuid();
            StartedAtUtc = DateTime.UtcNow;
            IsActive = true;
            IsAuthenticated = false;
            AccessToken = string.Empty;
            PlayerDisplayName = string.Empty;
        }

        public void Authenticate(string accessToken, string displayName)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("Cannot authenticate an inactive session.");
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            }

            AccessToken = accessToken;
            PlayerDisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            IsAuthenticated = true;
        }

        public void ClearAuthentication()
        {
            AccessToken = string.Empty;
            PlayerDisplayName = string.Empty;
            IsAuthenticated = false;
        }

        public void End()
        {
            IsActive = false;
            ClearAuthentication();
        }
    }
}
