using System;
using System.Collections.Generic;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Growth
{
    /// <summary>
    /// Evolução de espécie (definição) quando crescimento e vínculo atingem o limiar.
    /// </summary>
    public sealed class DragonEvolutionService
    {
        private static readonly Dictionary<string, string> Paths = new(StringComparer.Ordinal)
        {
            ["ember-whelp"] = "ash-drake",
            ["ash-drake"] = "portal-wyrm"
        };

        private readonly DragonSettings _settings;

        public DragonEvolutionService(DragonSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public static bool TryGetNextDefinition(string definitionId, out string nextId) =>
            Paths.TryGetValue(definitionId, out nextId!);

        public bool CanEvolve(DragonInstance dragon, out string error)
        {
            if (!TryGetNextDefinition(dragon.DefinitionId, out _))
            {
                error = "Esta espécie não possui evolução.";
                return false;
            }

            if (dragon.GrowthStage < _settings.EvolutionMinGrowthStage)
            {
                error = $"Crescimento insuficiente (mínimo {_settings.EvolutionMinGrowthStage}).";
                return false;
            }

            if (dragon.BondLevel < _settings.EvolutionMinBondLevel)
            {
                error = $"Vínculo insuficiente (mínimo nível {_settings.EvolutionMinBondLevel}).";
                return false;
            }

            if (dragon.State is DragonState.Deployed or DragonState.Hatching or DragonState.Locked
                or DragonState.Egg)
            {
                error = "Evolução indisponível neste estado.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryEvolve(DragonInstance dragon, out string error)
        {
            if (!CanEvolve(dragon, out error))
            {
                return false;
            }

            if (!TryGetNextDefinition(dragon.DefinitionId, out var nextId) ||
                !DragonCatalog.TryGet(nextId, out var nextDef))
            {
                error = "Definição evolutiva inválida.";
                return false;
            }

            dragon.DefinitionId = nextId;
            dragon.Hunger = Math.Min(dragon.Hunger, nextDef.MaxHunger);
            error = string.Empty;
            return true;
        }
    }
}
