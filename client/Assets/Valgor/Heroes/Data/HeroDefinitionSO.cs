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
            if (string.IsNullOrWhiteSpace(DisplayName)
                || DisplayName == PendingNamePlaceholder)
            {
                return Title;
            }

            return DisplayName;
        }
    }
}
