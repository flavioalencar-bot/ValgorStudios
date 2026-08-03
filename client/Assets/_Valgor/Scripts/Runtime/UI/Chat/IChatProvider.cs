using System;
using System.Collections.Generic;

namespace Valgor.UI.Chat
{
    public readonly struct ChatMessage
    {
        public ChatMessage(string channel, string author, string body, DateTime utc)
        {
            Channel = channel;
            Author = author;
            Body = body;
            Utc = utc;
        }

        public string Channel { get; }
        public string Author { get; }
        public string Body { get; }
        public DateTime Utc { get; }
    }

    public interface IChatProvider
    {
        IReadOnlyList<ChatMessage> GetMessages(string channel, int maxCount);
        void PushSystem(string body);
        IReadOnlyList<string> Channels { get; }
    }

    /// <summary>Chat local de sistema — sem conversa entre jogadores.</summary>
    public sealed class LocalSystemChatProvider : IChatProvider
    {
        private readonly List<ChatMessage> _messages = new();

        public IReadOnlyList<string> Channels { get; } = new[] { "Aliança", "Mundo", "Sistema" };

        public LocalSystemChatProvider()
        {
            PushSystem("Bem-vindo à Capital de Valgor.");
            PushSystem("Chat de jogadores: Em breve.");
        }

        public void PushSystem(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            _messages.Add(new ChatMessage("Sistema", "Sistema", body.Trim(), DateTime.UtcNow));
            if (_messages.Count > 80)
            {
                _messages.RemoveAt(0);
            }
        }

        public void PushEventLog(string body) => PushSystem(body);

        public IReadOnlyList<ChatMessage> GetMessages(string channel, int maxCount)
        {
            maxCount = Math.Max(1, maxCount);
            var result = new List<ChatMessage>(maxCount);
            for (var i = _messages.Count - 1; i >= 0 && result.Count < maxCount; i--)
            {
                var m = _messages[i];
                if (channel == "Sistema" || m.Channel == channel || m.Channel == "Sistema")
                {
                    if (channel == "Aliança" || channel == "Mundo")
                    {
                        // Sem backend: só mensagens de sistema nessas abas.
                        if (m.Channel != "Sistema")
                        {
                            continue;
                        }
                    }

                    result.Add(m);
                }
            }

            result.Reverse();
            return result;
        }
    }

    /// <summary>Stub futuro — não usar até existir backend.</summary>
    public sealed class RemoteChatProvider : IChatProvider
    {
        public IReadOnlyList<string> Channels { get; } = Array.Empty<string>();

        public IReadOnlyList<ChatMessage> GetMessages(string channel, int maxCount) =>
            Array.Empty<ChatMessage>();

        public void PushSystem(string body)
        {
        }
    }
}
