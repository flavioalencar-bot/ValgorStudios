using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Apresentação visual por faixa de tier do Castelo (escala relativa, offset, menu, câmera).
    /// Não equaliza footprints — cada tier mantém crescimento progressivo.
    /// </summary>
    public readonly struct CastleTierPresentation
    {
        public CastleTierPresentation(
            int tier,
            float localScaleMultiplier,
            Vector3 localOffset,
            float labelHeight,
            float menuGapBelow,
            float focusOrthoSize)
        {
            Tier = Math.Clamp(tier, 1, 6);
            LocalScaleMultiplier = Mathf.Max(0.01f, localScaleMultiplier);
            LocalOffset = localOffset;
            LabelHeight = Mathf.Max(1f, labelHeight);
            MenuGapBelow = Mathf.Max(8f, menuGapBelow);
            FocusOrthoSize = Mathf.Clamp(focusOrthoSize, 8f, 16f);
        }

        public int Tier { get; }
        public float LocalScaleMultiplier { get; }
        public Vector3 LocalOffset { get; }
        public float LabelHeight { get; }
        public float MenuGapBelow { get; }
        public float FocusOrthoSize { get; }

        public static CastleTierPresentation ForTier(int tier) =>
            Math.Clamp(tier, 1, 6) switch
            {
                1 => new(1, 1.00f, new Vector3(0f, 0.00f, 0f), 4.6f, 18f, 10.8f),
                2 => new(2, 1.00f, new Vector3(0f, 0.00f, 0f), 5.0f, 20f, 11.2f),
                3 => new(3, 1.00f, new Vector3(0f, 0.00f, 0f), 5.4f, 22f, 11.6f),
                4 => new(4, 0.98f, new Vector3(0f, 0.00f, 0f), 5.8f, 24f, 12.0f),
                5 => new(5, 0.96f, new Vector3(0f, 0.00f, 0f), 6.2f, 26f, 12.4f),
                _ => new(6, 0.94f, new Vector3(0f, 0.00f, 0f), 6.6f, 28f, 12.8f)
            };

        public static CastleTierPresentation ForBuildingLevel(int buildingLevel) =>
            ForTier(CastleRealVisualLoader.ResolveTier(buildingLevel));
    }
}
