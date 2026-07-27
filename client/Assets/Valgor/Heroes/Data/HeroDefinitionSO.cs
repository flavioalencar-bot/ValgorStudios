using UnityEngine;

namespace Valgor.Heroes.Data
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Hero Definition", fileName = "HeroDefinition")]
    public sealed class HeroDefinitionSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string Title;
        public string PendingNamePlaceholder = "A definir";
        public HeroRarity Rarity;
        public HeroFaction Faction;
        public string ClassName;
        public string Role;
        public string Position;
        public string ElementId;
        public string WeaponId;
        public BaseStats BaseStats = new();
        public SpecialPowerDefinitionSO SpecialPower;
        public string DefaultSkinId;
        public string PrefabAddress;
        public string PortraitAddress;

        public string ResolveDisplayName()
        {
            // Placeholder canônico: "A definir". Não comparar DisplayName com
            // PendingNamePlaceholder quando este foi gravado por engano com o nome real.
            const string CanonicalPending = "A definir";
            if (string.IsNullOrWhiteSpace(DisplayName)
                || DisplayName == CanonicalPending)
            {
                return Title;
            }

            return DisplayName;
        }
    }
}
