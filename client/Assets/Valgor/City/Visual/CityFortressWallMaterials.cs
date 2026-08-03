using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Material único da fortaleza — reutiliza o do Portão (WallGate).
    /// Sem novos estilos; fallback tonal se Resources ausente.
    /// </summary>
    public static class CityFortressWallMaterials
    {
        private static Material _shared;
        private static Material _wood;
        private static Material _metal;
        private static Material _banner;
        private static Material _fire;
        private static Material _walk;

        /// <summary>Pedra do Portão HF (shared).</summary>
        public static Material Stone
        {
            get
            {
                if (_shared != null)
                {
                    return _shared;
                }

                var fromResources = Resources.Load<Material>(CityFortressWallConfig.ResourcesMaterialKey);
                if (fromResources != null)
                {
                    _shared = fromResources;
                    return _shared;
                }

                // Fallback tonal alinhado ao WallGate (castanho-pedra).
                _shared = RuntimeSafeMaterials.Create(
                    new Color(0.52f, 0.45f, 0.38f),
                    "FortressWall_StoneFallback");
                _shared.enableInstancing = true;
                return _shared;
            }
        }

        public static Material Walk
        {
            get
            {
                if (_walk != null)
                {
                    return _walk;
                }

                _walk = RuntimeSafeMaterials.Create(
                    Color.Lerp(new Color(0.45f, 0.40f, 0.34f), Color.gray, 0.2f),
                    "FortressWall_Walk");
                _walk.enableInstancing = true;
                return _walk;
            }
        }

        public static Material Wood
        {
            get
            {
                if (_wood != null)
                {
                    return _wood;
                }

                _wood = RuntimeSafeMaterials.Create(CityVisualMaterials.Wood, "FortressWall_Wood");
                _wood.enableInstancing = true;
                return _wood;
            }
        }

        public static Material Metal
        {
            get
            {
                if (_metal != null)
                {
                    return _metal;
                }

                _metal = RuntimeSafeMaterials.Create(CityVisualMaterials.Gold, "FortressWall_Metal");
                _metal.enableInstancing = true;
                return _metal;
            }
        }

        public static Material Banner
        {
            get
            {
                if (_banner != null)
                {
                    return _banner;
                }

                _banner = RuntimeSafeMaterials.Create(new Color(0.55f, 0.14f, 0.12f), "FortressWall_Banner");
                _banner.enableInstancing = true;
                return _banner;
            }
        }

        public static Material Fire
        {
            get
            {
                if (_fire != null)
                {
                    return _fire;
                }

                _fire = RuntimeSafeMaterials.Create(new Color(1f, 0.45f, 0.15f), "FortressWall_Fire");
                _fire.enableInstancing = true;
                return _fire;
            }
        }

        public static void ApplyStone(Renderer renderer)
        {
            if (renderer != null)
            {
                renderer.sharedMaterial = Stone;
            }
        }
    }
}
