using UnityEngine;
using Valgor.City.Buildings;
using Valgor.City.Data;

namespace Valgor.City.Visual
{
    /// <summary>
    /// API de layout da City. Coordenadas oficiais em <see cref="CityCapitalLayoutConfig"/>.
    /// </summary>
    public static class CityLayout
    {
        public static Vector3 WorldPosition(string buildingId)
        {
            var p = CityCapitalLayoutConfig.GetPosition(buildingId);
            return new Vector3(p.X, p.Y, p.Z);
        }

        public static Quaternion WorldRotation(string buildingId)
        {
            if (!CityCapitalLayoutConfig.TryGet(buildingId, out var entry))
            {
                return Quaternion.identity;
            }

            return Quaternion.Euler(0f, entry.YawDegrees, 0f);
        }

        public static CityDistrict DistrictOf(string buildingId) =>
            CityCapitalLayoutConfig.GetDistrict(buildingId);

        public static Color IdentityColor(string buildingId)
        {
            var id = BuildingIdMigration.ToCanonical(buildingId);

            // Sprint 3 — identidade neutra (pedra/madeira); evita silhuetas “cubo colorido”.
            if (CityArchitectureSprint3Config.Enabled)
            {
                return id switch
                {
                    "castle" => new Color(0.52f, 0.5f, 0.46f),
                    "dragon-tower" => new Color(0.32f, 0.28f, 0.3f),
                    "wall" => new Color(0.5f, 0.48f, 0.44f),
                    "farm" => new Color(0.42f, 0.38f, 0.28f),
                    "lumbermill" => new Color(0.44f, 0.34f, 0.22f),
                    "quarry" => new Color(0.52f, 0.5f, 0.46f),
                    "mine" => new Color(0.4f, 0.38f, 0.36f),
                    "warehouse" => new Color(0.46f, 0.38f, 0.28f),
                    "residence" => new Color(0.5f, 0.46f, 0.4f),
                    "academy" => new Color(0.48f, 0.46f, 0.44f),
                    "hero-hall" => new Color(0.5f, 0.46f, 0.4f),
                    "hospital" => new Color(0.54f, 0.52f, 0.48f),
                    "market" => new Color(0.48f, 0.4f, 0.3f),
                    "temple" => new Color(0.52f, 0.48f, 0.42f),
                    "arena" => new Color(0.5f, 0.44f, 0.38f),
                    "training-ground" => new Color(0.44f, 0.4f, 0.34f),
                    "foundry" => new Color(0.4f, 0.34f, 0.3f),
                    _ => new Color(0.46f, 0.44f, 0.4f)
                };
            }

            return id switch
            {
                "castle" => new Color(0.52f, 0.5f, 0.46f),
                "dragon-tower" => new Color(0.28f, 0.22f, 0.3f),
                "wall" => new Color(0.5f, 0.48f, 0.44f),
                "farm" => new Color(0.4f, 0.5f, 0.28f),
                "lumbermill" => new Color(0.45f, 0.3f, 0.16f),
                "quarry" => new Color(0.55f, 0.54f, 0.5f),
                "mine" => new Color(0.36f, 0.36f, 0.4f),
                "warehouse" => new Color(0.5f, 0.38f, 0.24f),
                "residence" => new Color(0.48f, 0.42f, 0.34f),
                "academy" => new Color(0.38f, 0.44f, 0.55f),
                "hero-hall" => new Color(0.46f, 0.42f, 0.38f),
                "hospital" => new Color(0.58f, 0.55f, 0.52f),
                "market" => new Color(0.55f, 0.4f, 0.26f),
                "temple" => new Color(0.55f, 0.5f, 0.4f),
                "arena" => new Color(0.55f, 0.36f, 0.3f),
                "training-ground" => new Color(0.42f, 0.36f, 0.3f),
                "foundry" => new Color(0.4f, 0.28f, 0.22f),
                _ => new Color(0.44f, 0.42f, 0.4f)
            };
        }

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
