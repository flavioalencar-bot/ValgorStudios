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
            // Economia (O / N)
            ["farm"] = new Vector2(-12f, -2.5f),
            ["lumbermill"] = new Vector2(-12f, 4f),
            ["quarry"] = new Vector2(-8.5f, 10.5f),
            ["mine"] = new Vector2(-1.5f, 12f),
            // Comércio / suporte (S)
            ["market"] = new Vector2(0f, -10.5f),
            ["warehouse"] = new Vector2(6.5f, -10f),
            ["hospital"] = new Vector2(-6.5f, -10f),
            ["laboratory"] = new Vector2(3.5f, -12f),
            ["institute"] = new Vector2(-3.5f, -12f),
            // Militar (L)
            ["arena"] = new Vector2(11.5f, 3f),
            ["academy"] = new Vector2(11f, -4.5f),
            // Místico / marco (NE)
            ["temple"] = new Vector2(4.5f, 11f),
            ["dragon-tower"] = new Vector2(9.2f, 9.2f)
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
