#if UNITY_EDITOR
using UnityEngine;
using Valgor.Heroes.Characters;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.EditorTools
{
    public static class HeroAvatarValidator
    {
        public static bool Validate(string modelPath, out string message)
        {
            var report = VortexAssetImportValidator.ValidateAll();
            message = report.ToString();
            return !report.HasSourceModel || report.AvatarOk;
        }
    }

    public static class HeroTextureBudgetValidator
    {
        public static bool Validate(out string message)
        {
            var report = VortexAssetImportValidator.ValidateAll();
            message = report.ToString();
            return !report.HasSourceModel || report.TexturesOk;
        }
    }

    public static class HeroMaterialValidator
    {
        public static bool Validate(out string message)
        {
            var report = VortexAssetImportValidator.ValidateAll();
            message = report.ToString();
            return report.MaterialsOk;
        }
    }

    public static class HeroSocketValidator
    {
        public static bool Validate(GameObject prefab, out string message)
        {
            if (prefab == null)
            {
                message = "Prefab nulo.";
                return false;
            }

            var sockets = prefab.GetComponent<HeroSocketRegistry>();
            if (sockets == null)
            {
                message = "HeroSocketRegistry ausente.";
                return false;
            }

            var ok = sockets.HasAllRequired(out var missing);
            message = ok ? "Sockets OK." : "Faltando: " + string.Join(", ", missing);
            return ok;
        }
    }

    public static class HeroAnimationValidator
    {
        public static bool Validate(out string message)
        {
            var report = VortexAssetImportValidator.ValidateAll();
            message = report.ToString();
            return report.AnimationsOk;
        }
    }

    public static class HeroPrefabBuilder
    {
        public static GameObject BuildVortex() => VortexPrefabBuilder.BuildOrUpdate();
    }
}
#endif
