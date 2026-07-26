using System;
using Valgor.WorldMap.Creatures;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Energy
{
    public enum EnergyActionKind
    {
        DispatchMarch,
        EngageCreature
    }

    /// <summary>
    /// Resolve custos de energia a partir de dados configuráveis (sem magic numbers no fluxo).
    /// </summary>
    public sealed class EnergyCostResolver
    {
        private readonly EnergySettings _settings;

        public EnergyCostResolver(EnergySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public int Resolve(EnergyActionKind action, string? targetId = null)
        {
            return action switch
            {
                EnergyActionKind.DispatchMarch => Math.Max(0, _settings.MarchDispatchCost),
                EnergyActionKind.EngageCreature => ResolveCreature(targetId),
                _ => 0
            };
        }

        public int ResolveCreature(string? creatureId)
        {
            if (string.IsNullOrWhiteSpace(creatureId) ||
                !WorldCreatureCatalog.TryGet(creatureId, out var definition))
            {
                return 0;
            }

            return Math.Max(0, definition.EnergyCost);
        }

        public int ResolveMarchDispatch() => Math.Max(0, _settings.MarchDispatchCost);
    }
}
