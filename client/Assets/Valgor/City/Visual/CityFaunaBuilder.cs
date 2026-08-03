using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Fauna leve Art Pass 2.0 — opcional por qualidade.</summary>
    public static class CityFaunaBuilder
    {
        public static void Build(Transform ambient)
        {
            if (ambient == null)
            {
                return;
            }

            if (CityAmbientQualitySettings.AllowBirds)
            {
                Bird(ambient, "BirdA", new Vector3(-8f, 6.2f, 4f), new Color(0.2f, 0.2f, 0.22f));
                Bird(ambient, "BirdB", new Vector3(6f, 7.1f, -2f), new Color(0.22f, 0.2f, 0.2f));
                Bird(ambient, "CrowA", new Vector3(2f, 8f, 12f), new Color(0.12f, 0.12f, 0.14f));
            }

            if (CityAmbientQualitySettings.AllowButterflies)
            {
                Critter(ambient, "ButterflyA", new Vector3(-5f, 1.4f, -4f), new Color(0.9f, 0.55f, 0.2f), 0.16f);
                Critter(ambient, "ButterflyB", new Vector3(4.5f, 1.5f, 3f), new Color(0.55f, 0.35f, 0.85f), 0.14f);
            }

            if (CityAmbientQualitySettings.AllowFishHints && CityAmbientQualitySettings.AllowWaterFx)
            {
                Critter(ambient, "FishA", new Vector3(-24.2f, -0.05f, 2f), new Color(0.4f, 0.55f, 0.6f), 0.22f);
                Critter(ambient, "FishB", new Vector3(24.3f, -0.05f, -3f), new Color(0.35f, 0.5f, 0.55f), 0.2f);
            }

            if (CityAmbientQualitySettings.AllowFireflies)
            {
                Critter(ambient, "FireflyA", new Vector3(-3f, 1.8f, 5f), new Color(0.75f, 1f, 0.35f), 0.1f, emissive: true);
                Critter(ambient, "FireflyB", new Vector3(5f, 2.0f, -2f), new Color(0.8f, 1f, 0.4f), 0.1f, emissive: true);
            }
        }

        private static void Bird(Transform root, string name, Vector3 pos, Color color)
        {
            var go = Critter(root, name, pos, color, 0.22f);
            var mover = go.AddComponent<CityFaunaOrbit>();
            mover.Configure(pos, 2.5f, 0.35f);
        }

        private static GameObject Critter(
            Transform root,
            string name,
            Vector3 pos,
            Color color,
            float size,
            bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * size;
            Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityArtMaterialLibrary.ApplyVariant(go.GetComponent<Renderer>(), CityArtMaterialCategory.Metal, color, 0.9f);
            if (emissive)
            {
                CityVisualMaterials.ApplyEmissiveHint(go.GetComponent<Renderer>(), color, 0.55f);
            }

            return go;
        }
    }

    public sealed class CityFaunaOrbit : MonoBehaviour
    {
        private Vector3 _center;
        private float _radius = 2f;
        private float _speed = 0.4f;
        private float _angle;

        public void Configure(Vector3 center, float radius, float speed)
        {
            _center = center;
            _radius = radius;
            _speed = speed;
            _angle = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            _angle += Time.deltaTime * _speed;
            transform.localPosition = _center + new Vector3(Mathf.Cos(_angle) * _radius, Mathf.Sin(_angle * 1.7f) * 0.35f,
                Mathf.Sin(_angle) * _radius);
        }
    }
}
