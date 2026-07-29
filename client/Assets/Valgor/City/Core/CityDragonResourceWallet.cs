using Valgor.City.Data;
using Valgor.Core.Modules;

namespace Valgor.City.Core
{
    /// <summary>
    /// Adaptador da carteira da cidade para alimentação de dragões.
    /// </summary>
    public sealed class CityDragonResourceWallet : IDragonResourceWallet
    {
        private readonly ResourceWallet _wallet;

        public CityDragonResourceWallet(ResourceWallet wallet) =>
            _wallet = wallet ?? throw new System.ArgumentNullException(nameof(wallet));

        public long GetFood() => _wallet.Get(ResourceType.Food);

        public long GetDragonEssence() => _wallet.Get(ResourceType.DragonEssence);

        public long GetDiamonds() => _wallet.Get(ResourceType.Diamonds);

        public bool TrySpendFood(long amount) => _wallet.TrySpend(ResourceType.Food, amount);

        public bool TrySpendDragonEssence(long amount) => _wallet.TrySpend(ResourceType.DragonEssence, amount);

        public bool TrySpendDiamonds(long amount) => _wallet.TrySpend(ResourceType.Diamonds, amount);
    }
}
