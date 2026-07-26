using System.Collections.Generic;
using UnityEngine;

namespace Valgor.Heroes.Data
{
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
}
