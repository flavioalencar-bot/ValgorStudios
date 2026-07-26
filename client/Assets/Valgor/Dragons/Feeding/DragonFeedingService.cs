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

        public DragonFeedingService(DragonSettings settings, DragonStateMachine stateMachine)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        public bool CanFeed(DragonInstance dragon, IDragonResourceWallet wallet, out string error)
        {
            if (dragon.State is not (DragonState.Hungry or DragonState.Resting or DragonState.Ready))
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
            if (dragon.State == DragonState.Hungry)
            {
                var next = dragon.Hunger >= definition.MaxHunger / 2 ? DragonState.Ready : DragonState.Resting;
                if (!_stateMachine.TryTransition(dragon, next, out error))
                {
                    return false;
                }
            }
            else if (dragon.State == DragonState.Resting && dragon.Hunger >= definition.MaxHunger / 2)
            {
                if (!_stateMachine.TryTransition(dragon, DragonState.Ready, out error))
                {
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }
}
