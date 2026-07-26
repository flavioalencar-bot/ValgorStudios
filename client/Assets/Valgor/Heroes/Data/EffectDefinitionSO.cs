using System.Collections.Generic;
using UnityEngine;
using Valgor.Heroes.Magic;

namespace Valgor.Heroes.Data
{
    [CreateAssetMenu(menuName = "Valgor/Heroes/Effect Definition", fileName = "EffectDefinition")]
    public sealed class EffectDefinitionSO : ScriptableObject
    {
        public string Description;
        public EffectKind Kind = EffectKind.Buff;
    }
}
