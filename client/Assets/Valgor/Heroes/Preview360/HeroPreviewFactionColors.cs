using UnityEngine;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Preview360
{
    public static class HeroPreviewFactionColors
    {
        public static readonly Color RosaDeSangue = new(0.55f, 0.08f, 0.12f);
        public static readonly Color AsasDoAmanhecer = new(0.18f, 0.42f, 0.86f);
        public static readonly Color GuardaDaOrdem = new(0.86f, 0.70f, 0.18f);

        public static Color ForFaction(HeroFaction faction) => faction switch
        {
            HeroFaction.RosaDeSangue => RosaDeSangue,
            HeroFaction.AsasDoAmanhecer => AsasDoAmanhecer,
            HeroFaction.GuardaDaOrdem => GuardaDaOrdem,
            _ => Color.gray
        };
    }
}
