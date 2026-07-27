using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Data;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Distritos da cidade: Castelo no centro, economia a oeste/norte,
    /// comércio ao sul, militar a leste, místico/NE com Torre.
    /// </summary>
    public static class CityLayout
    {
        private static readonly Dictionary<string, Vector2> Positions = new()
        {
            ["castle"] = new Vector2(0f, 0f),
            // Economia (O / N) — agrupada e mais próxima do centro
            ["farm"] = new Vector2(-10.5f, -2f),
            ["lumbermill"] = new Vector2(-10.5f, 3.5f),
            ["quarry"] = new Vector2(-7.5f, 9.5f),
            ["mine"] = new Vector2(-1.2f, 10.8f),
            // Comércio / suporte (S)
            ["market"] = new Vector2(0f, -9.5f),
            ["warehouse"] = new Vector2(5.8f, -9.2f),
            ["hospital"] = new Vector2(-5.8f, -9.2f),
            ["laboratory"] = new Vector2(3.2f, -11.2f),
            ["institute"] = new Vector2(-3.2f, -11.2f),
            // Militar (L)
            ["arena"] = new Vector2(10.5f, 2.8f),
            ["academy"] = new Vector2(10.2f, -3.8f),
            // Místico / marco (NE)
            ["temple"] = new Vector2(4.2f, 10.2f),
            ["dragon-tower"] = new Vector2(8.6f, 8.6f)
        };

        public static Vector3 WorldPosition(string buildingId)
        {
            if (!Positions.TryGetValue(buildingId, out var xz))
            {
                xz = Vector2.zero;
            }

            return new Vector3(xz.x, 0f, xz.y);
        }

        public static Color IdentityColor(string buildingId) => buildingId switch
        {
            "castle" => new Color(0.52f, 0.5f, 0.46f),
            "dragon-tower" => new Color(0.32f, 0.26f, 0.34f),
            "farm" => new Color(0.4f, 0.5f, 0.28f),
            "lumbermill" => new Color(0.45f, 0.3f, 0.16f),
            "quarry" => new Color(0.55f, 0.54f, 0.5f),
            "mine" => new Color(0.36f, 0.36f, 0.4f),
            "warehouse" => new Color(0.5f, 0.38f, 0.24f),
            "academy" => new Color(0.38f, 0.44f, 0.55f),
            "institute" => new Color(0.4f, 0.45f, 0.52f),
            "hospital" => new Color(0.58f, 0.55f, 0.52f),
            "market" => new Color(0.55f, 0.4f, 0.26f),
            "temple" => new Color(0.55f, 0.5f, 0.4f),
            "arena" => new Color(0.55f, 0.36f, 0.3f),
            "laboratory" => new Color(0.32f, 0.48f, 0.5f),
            _ => new Color(0.44f, 0.42f, 0.4f)
        };

        public static BuildingStateTint ToTint(BuildingState state) => state switch
        {
            BuildingState.Ready => BuildingStateTint.Ready,
            BuildingState.Available => BuildingStateTint.Available,
            BuildingState.Locked => BuildingStateTint.Locked,
            BuildingState.Upgrading => BuildingStateTint.Available,
            _ => BuildingStateTint.Ready
        };
    }
}
