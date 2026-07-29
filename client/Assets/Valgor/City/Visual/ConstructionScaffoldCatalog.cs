using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    public enum ConstructionScaffoldSize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        Castle = 3,
        Wall = 4
    }

    /// <summary>Mapeia edifício → kit de andaime e ajustes locais (só apresentação).</summary>
    public static class ConstructionScaffoldCatalog
    {
        public static ConstructionScaffoldSize ResolveSize(string buildingDefinitionId) =>
            buildingDefinitionId switch
            {
                "castle" => ConstructionScaffoldSize.Castle,
                "wall" => ConstructionScaffoldSize.Wall,
                "dragon-tower" => ConstructionScaffoldSize.Large,
                "warehouse" => ConstructionScaffoldSize.Medium,
                "academy" => ConstructionScaffoldSize.Medium,
                "hospital" => ConstructionScaffoldSize.Medium,
                "arena" => ConstructionScaffoldSize.Large,
                "temple" => ConstructionScaffoldSize.Medium,
                "laboratory" => ConstructionScaffoldSize.Medium,
                "institute" => ConstructionScaffoldSize.Medium,
                "market" => ConstructionScaffoldSize.Medium,
                "farm" => ConstructionScaffoldSize.Small,
                "lumbermill" => ConstructionScaffoldSize.Small,
                "quarry" => ConstructionScaffoldSize.Small,
                "mine" => ConstructionScaffoldSize.Small,
                _ => ConstructionScaffoldSize.Medium
            };

        public static string PrefabResourceKey(ConstructionScaffoldSize size) => size switch
        {
            ConstructionScaffoldSize.Small => "Valgor/Construction/ConstructionScaffold_Small",
            ConstructionScaffoldSize.Medium => "Valgor/Construction/ConstructionScaffold_Medium",
            ConstructionScaffoldSize.Large => "Valgor/Construction/ConstructionScaffold_Large",
            ConstructionScaffoldSize.Castle => "Valgor/Construction/ConstructionScaffold_Castle",
            ConstructionScaffoldSize.Wall => "Valgor/Construction/ConstructionScaffold_Wall",
            _ => "Valgor/Construction/ConstructionScaffold_Medium"
        };

        public static string PrefabAssetName(ConstructionScaffoldSize size) => size switch
        {
            ConstructionScaffoldSize.Small => "ConstructionScaffold_Small",
            ConstructionScaffoldSize.Medium => "ConstructionScaffold_Medium",
            ConstructionScaffoldSize.Large => "ConstructionScaffold_Large",
            ConstructionScaffoldSize.Castle => "ConstructionScaffold_Castle",
            ConstructionScaffoldSize.Wall => "ConstructionScaffold_Wall",
            _ => "ConstructionScaffold_Medium"
        };

        /// <summary>Escala local relativa ao footprint do edifício (ajuste fino).</summary>
        public static Vector3 LocalScaleMultiplier(string buildingDefinitionId) =>
            buildingDefinitionId switch
            {
                "castle" => new Vector3(1.05f, 1.1f, 1.05f),
                "wall" => new Vector3(1.2f, 0.95f, 0.55f),
                "dragon-tower" => new Vector3(0.95f, 1.15f, 0.95f),
                "farm" => new Vector3(1.1f, 0.85f, 1.1f),
                _ => Vector3.one
            };

        public static Vector3 LocalOffset(string buildingDefinitionId) =>
            buildingDefinitionId switch
            {
                "wall" => new Vector3(0f, 0f, 0.35f),
                "castle" => new Vector3(0f, 0.05f, 0f),
                _ => Vector3.zero
            };
    }
}
