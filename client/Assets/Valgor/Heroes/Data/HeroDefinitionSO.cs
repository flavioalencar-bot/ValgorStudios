using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Heroes.Data
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Effect Definition", fileName = "EffectDefinition")]
    public sealed class EffectDefinitionSO : ScriptableObject
    {
        public string Description;
        public Magic.EffectKind Kind = Magic.EffectKind.Buff;
    }

    [CreateAssetMenu(menuName = "Valgor/Heroes/Special Power", fileName = "SpecialPowerDefinition")]
    public sealed class SpecialPowerDefinitionSO : ScriptableObject
    {
        public string Id;
        public string HeroId;
        public string DisplayName;
        public float ActiveDurationSec;
        public float CooldownSec;
        public TargetType TargetType = TargetType.SelfOrAllies;
        public bool Interruptible = true;
        public bool CanActivateWhileControlled;
        public List<EffectDefinitionSO> Effects = new();
        public string AnimationState = "Special";
        public string VfxAddress;
        public string SfxAddress;
    }

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

    [CreateAssetMenu(menuName = "Valgor/Heroes/Hero Catalog", fileName = "HeroCatalog")]
    public sealed class HeroCatalogSO : ScriptableObject
    {
        public string Version = "1.0.0";
        public List<HeroDefinitionSO> Heroes = new();
    }
}
