using System;
using System.Collections.Generic;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    public sealed class WorldNodeSelectionService
    {
        public WorldNodeInstance? Selected { get; private set; }

        /// <summary>ID persistível (nunca referência Unity).</summary>
        public string? SelectedNodeId { get; private set; }

        public event Action<WorldNodeInstance?>? SelectionChanged;

        public void Select(WorldNodeInstance node)
        {
            Selected = node ?? throw new ArgumentNullException(nameof(node));
            SelectedNodeId = node.DefinitionId;
            SelectionChanged?.Invoke(Selected);
        }

        public void Deselect()
        {
            Selected = null;
            SelectedNodeId = null;
            SelectionChanged?.Invoke(null);
        }

        /// <summary>
        /// Resolve a seleção a partir do ID após recarregar os nós (sem manter referência antiga).
        /// </summary>
        public void RestoreFromId(string? nodeId, IReadOnlyDictionary<string, WorldNodeInstance> nodes)
        {
            if (string.IsNullOrWhiteSpace(nodeId) ||
                nodes == null ||
                !nodes.TryGetValue(nodeId, out var instance) ||
                instance.Status == WorldNodeStatus.Locked)
            {
                Deselect();
                return;
            }

            Select(instance);
        }
    }
}
