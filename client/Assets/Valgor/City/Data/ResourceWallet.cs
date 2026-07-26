using System;
using System.Collections.Generic;

namespace Valgor.City.Data
{
    public enum ResourceType
    {
        Gold,
        Food,
        Wood,
        Stone,
        Iron,
        DragonEssence,
        Diamonds
    }

    public sealed class ResourceChangedEvent : EventArgs
    {
        public ResourceChangedEvent(ResourceType resource, long previousAmount, long currentAmount)
        {
            Resource = resource;
            PreviousAmount = previousAmount;
            CurrentAmount = currentAmount;
        }

        public ResourceType Resource { get; }
        public long PreviousAmount { get; }
        public long CurrentAmount { get; }
    }

    public sealed class ResourceWallet
    {
        private readonly Dictionary<ResourceType, long> _amounts = new();

        public event EventHandler<ResourceChangedEvent>? Changed;

        public long Get(ResourceType resource) => _amounts.TryGetValue(resource, out var amount) ? amount : 0;

        public void Add(ResourceType resource, long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Set(resource, checked(Get(resource) + amount));
        }

        public bool TrySpend(ResourceType resource, long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var current = Get(resource);
            if (current < amount)
            {
                return false;
            }

            Set(resource, current - amount);
            return true;
        }

        private void Set(ResourceType resource, long amount)
        {
            var previous = Get(resource);
            _amounts[resource] = amount;
            Changed?.Invoke(this, new ResourceChangedEvent(resource, previous, amount));
        }
    }
}
