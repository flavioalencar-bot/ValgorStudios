using System;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Locate
{
    /// <summary>
    /// Resolve alvos de localização e pedidos de foco de câmera (sem mover a câmera diretamente).
    /// </summary>
    public sealed class WorldMapLocatorService
    {
        private readonly WorldMapSettings _settings;
        private readonly Func<string, WorldMapNodeDefinition> _getDefinition;
        private readonly Func<string, WorldNodeInstance?> _tryGetNode;
        private readonly Func<WorldNodeInstance?> _getSelected;
        private readonly Func<string?> _getActiveMarchTargetId;

        public WorldMapLocatorService(
            WorldMapSettings settings,
            Func<string, WorldMapNodeDefinition> getDefinition,
            Func<string, WorldNodeInstance?> tryGetNode,
            Func<WorldNodeInstance?> getSelected,
            Func<string?> getActiveMarchTargetId)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _getDefinition = getDefinition ?? throw new ArgumentNullException(nameof(getDefinition));
            _tryGetNode = tryGetNode ?? throw new ArgumentNullException(nameof(tryGetNode));
            _getSelected = getSelected ?? throw new ArgumentNullException(nameof(getSelected));
            _getActiveMarchTargetId = getActiveMarchTargetId ?? throw new ArgumentNullException(nameof(getActiveMarchTargetId));
        }

        public bool TryLocatePlayerHome(out WorldMapLocationTarget target, out string error) =>
            TryLocateNode(_settings.PlayerHomeNodeId, WorldMapLocationKind.PlayerHome, out target, out error);

        public bool TryLocateActiveMarch(out WorldMapLocationTarget target, out string error)
        {
            var marchTargetId = _getActiveMarchTargetId();
            if (string.IsNullOrWhiteSpace(marchTargetId))
            {
                target = null!;
                error = "Nenhuma marcha ativa.";
                return false;
            }

            return TryLocateNode(marchTargetId, WorldMapLocationKind.ActiveMarch, out target, out error);
        }

        public bool TryLocateSelectedNode(out WorldMapLocationTarget target, out string error)
        {
            var selected = _getSelected();
            if (selected == null)
            {
                target = null!;
                error = "Nenhum nó selecionado.";
                return false;
            }

            return TryLocateNode(selected.DefinitionId, WorldMapLocationKind.SelectedNode, out target, out error);
        }

        public bool TryLocateCreature(string creatureNodeId, out WorldMapLocationTarget target, out string error)
        {
            if (!TryLocateNode(creatureNodeId, WorldMapLocationKind.Creature, out target, out error))
            {
                return false;
            }

            if (_getDefinition(creatureNodeId).Kind != WorldNodeKind.Creature)
            {
                error = "O alvo não é uma criatura.";
                return false;
            }

            return true;
        }

        public bool TryLocateResource(string resourceNodeId, out WorldMapLocationTarget target, out string error)
        {
            if (!TryLocateNode(resourceNodeId, WorldMapLocationKind.Resource, out target, out error))
            {
                return false;
            }

            if (_getDefinition(resourceNodeId).Kind != WorldNodeKind.Resource)
            {
                error = "O alvo não é um recurso.";
                return false;
            }

            return true;
        }

        public WorldCameraFocusRequest CreateFocusRequest(WorldMapLocationTarget target, float? zoomOverride = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var zoom = zoomOverride ?? _settings.LocateDefaultZoom;
            return new WorldCameraFocusRequest(target.X, target.Z, zoom);
        }

        private bool TryLocateNode(
            string nodeId,
            WorldMapLocationKind kind,
            out WorldMapLocationTarget target,
            out string error)
        {
            target = null!;
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                error = "Identificador de nó inválido.";
                return false;
            }

            var node = _tryGetNode(nodeId);
            if (node == null)
            {
                error = "Nó não encontrado.";
                return false;
            }

            var definition = _getDefinition(nodeId);
            target = new WorldMapLocationTarget(kind, definition.Id, definition.DisplayName, definition.X, definition.Z);
            error = string.Empty;
            return true;
        }
    }
}
