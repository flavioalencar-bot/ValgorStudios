using System;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Feeding
{
    public sealed class DragonFeedingService
    {
        private readonly DragonSettings _settings;
        private readonly DragonStateMachine _stateMachine;
        private readonly DragonHungerService _hunger;

        public DragonFeedingService(
            DragonSettings settings,
            DragonStateMachine stateMachine,
            DragonHungerService hunger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _hunger = hunger ?? throw new ArgumentNullException(nameof(hunger));
        }

        public bool CanFeed(DragonInstance dragon, IDragonResourceWallet wallet, out string error)
        {
            if (dragon.State is not (DragonState.Hungry or DragonState.Resting or DragonState.Ready or DragonState.Juvenile))
            {
                error = "Dragão não pode ser alimentado neste estado.";
                return false;
            }

            if (wallet.GetFood() < _settings.FeedFoodCost ||
                wallet.GetDragonEssence() < _settings.FeedEssenceCost)
            {
                error = "Recursos insuficientes para alimentar.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryFeed(
            DragonInstance dragon,
            DragonDefinition definition,
            IDragonResourceWallet wallet,
            out string error)
        {
            if (!CanFeed(dragon, wallet, out error))
            {
                return false;
            }

            if (!wallet.TrySpendFood(_settings.FeedFoodCost) ||
                !wallet.TrySpendDragonEssence(_settings.FeedEssenceCost))
            {
                error = "Falha ao debitar recursos.";
                return false;
            }

            dragon.Hunger = Math.Min(definition.MaxHunger, dragon.Hunger + _settings.FeedHungerRestore);
            var ready = _hunger.IsReadyHunger(dragon, definition);

            if (dragon.State == DragonState.Hungry)
            {
                var next = ready ? DragonState.Ready : DragonState.Resting;
                if (!_stateMachine.TryTransition(dragon, next, out error))
                {
                    return false;
                }

                if (next == DragonState.Resting)
                {
                    dragon.StateEndsAtUtc = DateTime.UtcNow.AddHours(_settings.RestDurationHours);
                }
                else
                {
                    dragon.StateEndsAtUtc = null;
                }
            }
            else if (dragon.State == DragonState.Resting && ready)
            {
                if (!_stateMachine.TryTransition(dragon, DragonState.Ready, out error))
                {
                    return false;
                }

                dragon.StateEndsAtUtc = null;
            }

            error = string.Empty;
            return true;
        }
    }
}
