using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Ambientes sonoros por distrito (estrutura).
    /// Clips podem ser ligados depois — volumes/zonas já respeitam qualidade.
    /// </summary>
    public static class CityDistrictAmbienceAudio
    {
        public static void Build(Transform root)
        {
            if (root == null || !CityAmbientQualitySettings.AllowDistrictAudio)
            {
                return;
            }

            var host = new GameObject("DistrictAmbience").transform;
            host.SetParent(root, false);

            Zone(host, "mercado", new Vector3(-8f, 1.5f, -6.5f), 6f);
            Zone(host, "praca", new Vector3(0f, 1.5f, CityCapitalEnvironmentConfig.PlazaCenterZ), 7f);
            Zone(host, "castelo", new Vector3(0f, 2f, 0.5f), 6f);
            Zone(host, "templo", new Vector3(9f, 1.5f, 4.8f), 5f);
            Zone(host, "fazenda", new Vector3(-14f, 1.2f, -7.5f), 5.5f);
            Zone(host, "forja", new Vector3(13f, 1.5f, -13f), 5f);
            Zone(host, "rio", new Vector3(-24f, 0.8f, 0f), 8f);
            Zone(host, "torre-dragoes", new Vector3(0f, 3f, 17.6f), 6.5f);
        }

        private static void Zone(Transform parent, string id, Vector3 pos, float radius)
        {
            var go = new GameObject($"Ambience_{id}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = radius * 0.35f;
            src.maxDistance = radius * 1.8f;
            src.volume = CityAmbientQualitySettings.Current == CityAmbientQuality.High ? 0.35f : 0.22f;
            // Sem clip nesta sprint — zona pronta para assets.
        }
    }
}
