using System;

namespace Valgor.WorldMap.Marches
{
    public sealed class MarchSnapshot
    {
        public DateTime SavedAtUtc { get; set; }
        public DateTime LastAdvanceUtc { get; set; }
        public MarchOrder? March { get; set; }
    }

    public interface IMarchRepository
    {
        MarchSnapshot? Load();
        void Save(MarchSnapshot snapshot);
    }

    /// <summary>
    /// Persistência técnica da marcha ativa (memória da instância + contrato para backend).
    /// </summary>
    public sealed class MarchRepository : IMarchRepository
    {
        private MarchSnapshot? _memory;

        public MarchSnapshot? Load() => _memory == null ? null : Clone(_memory);

        public void Save(MarchSnapshot snapshot) => _memory = Clone(snapshot);

        private static MarchSnapshot Clone(MarchSnapshot source) =>
            new()
            {
                SavedAtUtc = source.SavedAtUtc,
                LastAdvanceUtc = source.LastAdvanceUtc,
                March = source.March?.Clone()
            };
    }
}
