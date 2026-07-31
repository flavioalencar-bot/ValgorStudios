using System;
using System.Collections.Generic;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Deployment
{
    public sealed class DragonDeploymentService
    {
        private readonly DragonStateMachine _stateMachine;
        private readonly Dictionary<string, string> _marchToDragon = new();

        public DragonDeploymentService(DragonStateMachine stateMachine)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        public IReadOnlyDictionary<string, string> MarchAssignments => _marchToDragon;

        public void RestoreAssignment(string marchId, string dragonId) =>
            _marchToDragon[marchId] = dragonId;

        public bool TryDeploy(DragonInstance dragon, string marchId, out string error)
        {
            if (string.IsNullOrWhiteSpace(marchId))
            {
                error = "Marcha inválida.";
                return false;
            }

            if (dragon.State != DragonState.Ready)
            {
                error = "Somente dragões READY podem ser destacados.";
                return false;
            }

            if (_marchToDragon.ContainsKey(marchId))
            {
                error = "Marcha já possui dragão destacado.";
                return false;
            }

            if (!_stateMachine.TryTransition(dragon, DragonState.Deployed, out error))
            {
                return false;
            }

            dragon.AssignedMarchId = marchId;
            _marchToDragon[marchId] = dragon.InstanceId;
            return true;
        }

        /// <summary>
        /// Combate ocorre sob DEPLOYED (sem estado separado). Mantém o contrato do World Map.
        /// </summary>
        public bool TryEnterCombat(DragonInstance dragon, out string error)
        {
            if (dragon.State != DragonState.Deployed)
            {
                error = "Dragão precisa estar destacado (DEPLOYED).";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryRecall(DragonInstance dragon, bool injured, out string error)
        {
            if (dragon.State != DragonState.Deployed)
            {
                error = "Dragão não está em missão.";
                return false;
            }

            var next = injured ? DragonState.Injured : DragonState.Exhausted;
            if (!_stateMachine.TryTransition(dragon, next, out error))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(dragon.AssignedMarchId))
            {
                _marchToDragon.Remove(dragon.AssignedMarchId);
            }

            dragon.AssignedMarchId = null;
            return true;
        }

        public bool TryGetDragonForMarch(string marchId, out string dragonId) =>
            _marchToDragon.TryGetValue(marchId, out dragonId!);

        public void ClearAllAssignments() => _marchToDragon.Clear();
    }
}
