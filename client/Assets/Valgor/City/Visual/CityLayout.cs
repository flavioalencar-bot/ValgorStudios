using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Data;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Paleta e posições da cidade provisional (sem arte final).
    /// </summary>
    public static class CityLayout
    {
        /// <summary>Posições XZ em metros — castelo no centro, distritos ao redor.</summary>
        private static readonly Dictionary<string, Vector2> Positions = new()
        {
            ["castle"] = new Vector2(0f, 0f),
            ["market"] = new Vector2(0f, -9.5f),
            ["warehouse"] = new Vector2(7f, -9f),
            ["hospital"] = new Vector2(-7f, -9f),
            ["farm"] = new Vector2(-11f, -3f),
            ["lumbermill"] = new Vector2(-11f, 4f),
            ["quarry"] = new Vector2(-7f, 10f),
            ["mine"] = new Vector2(0f, 11f),
            ["dragon-tower"] = new Vector2(9f, 9f),
            ["arena"] = new Vector2(11f, 2f),
            ["academy"] = new Vector2(11f, -4f),
            ["temple"] = new Vector2(6f, 11f),
            ["institute"] = new Vector2(-3f, -11.5f),
            ["laboratory"] = new Vector2(3f, -11.5f)
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
            "castle" => new Color(0.5f, 0.48f, 0.46f),
            "dragon-tower" => new Color(0.38f, 0.32f, 0.4f),
            "farm" => new Color(0.42f, 0.48f, 0.32f),
            "lumbermill" => new Color(0.42f, 0.3f, 0.18f),
            "quarry" => new Color(0.5f, 0.5f, 0.48f),
            "mine" => new Color(0.4f, 0.4f, 0.42f),
            "warehouse" => new Color(0.48f, 0.38f, 0.26f),
            "academy" => new Color(0.4f, 0.44f, 0.5f),
            "institute" => new Color(0.38f, 0.42f, 0.48f),
            "hospital" => new Color(0.55f, 0.54f, 0.52f),
            "market" => new Color(0.52f, 0.4f, 0.28f),
            "temple" => new Color(0.52f, 0.48f, 0.4f),
            "arena" => new Color(0.5f, 0.36f, 0.32f),
            "laboratory" => new Color(0.36f, 0.46f, 0.48f),
            _ => new Color(0.44f, 0.42f, 0.4f)
        };

        public static BuildingStateTint ToTint(BuildingState state) => state switch
        {
            BuildingState.Ready => BuildingStateTint.Ready,
            BuildingState.Available => BuildingStateTint.Available,
            BuildingState.Locked => BuildingStateTint.Locked,
            _ => BuildingStateTint.Ready
        };
    }
}
