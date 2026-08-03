using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>NPCs leves Art Pass 2.0 — rotas simples, sem IA.</summary>
    public static class CityNpcAmbientBuilder
    {
        public static void Build(Transform npcsRoot)
        {
            if (npcsRoot == null || !CityAmbientQualitySettings.AllowNpcs)
            {
                return;
            }

            Spawn(npcsRoot, "guarda", new Vector3(-2.4f, 0.55f, -18.5f), new Vector3(2.4f, 0.55f, -16.5f),
                new Color(0.35f, 0.38f, 0.45f));
            Spawn(npcsRoot, "guarda", new Vector3(2.4f, 0.55f, -18.5f), new Vector3(-2.2f, 0.55f, -17.8f),
                new Color(0.35f, 0.38f, 0.45f));
            Spawn(npcsRoot, "mercador", new Vector3(-8.5f, 0.55f, -7.2f), new Vector3(-6.2f, 0.55f, -5.8f),
                new Color(0.55f, 0.35f, 0.22f));
            Spawn(npcsRoot, "ferreiro", new Vector3(12.5f, 0.55f, -12.5f), new Vector3(13.8f, 0.55f, -11.2f),
                new Color(0.4f, 0.32f, 0.28f));
            Spawn(npcsRoot, "minerador", new Vector3(-14.5f, 1.2f, 11.5f), new Vector3(-13.2f, 1.2f, 12.4f),
                new Color(0.45f, 0.42f, 0.4f));
            Spawn(npcsRoot, "lenhador", new Vector3(-13.5f, 0.55f, -13f), new Vector3(-12.2f, 0.55f, -11.5f),
                new Color(0.42f, 0.3f, 0.18f));
            Spawn(npcsRoot, "fazendeiro", new Vector3(-14.2f, 0.55f, -7.5f), new Vector3(-12.8f, 0.55f, -6.2f),
                new Color(0.4f, 0.48f, 0.28f));
            Spawn(npcsRoot, "sacerdote", new Vector3(9.0f, 0.55f, 4.8f), new Vector3(8.2f, 0.55f, 5.6f),
                new Color(0.75f, 0.72f, 0.68f));
            Spawn(npcsRoot, "curandeiro", new Vector3(-9.2f, 0.55f, 4.2f), new Vector3(-8.4f, 0.55f, 5.0f),
                new Color(0.55f, 0.7f, 0.65f));
            Spawn(npcsRoot, "mensageiro", new Vector3(0f, 0.55f, -8f), new Vector3(0.5f, 0.55f, -4.5f),
                new Color(0.5f, 0.25f, 0.22f));
        }

        private static void Spawn(Transform root, string category, Vector3 a, Vector3 b, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Npc_{category}";
            go.transform.SetParent(root, false);
            go.transform.localPosition = a;
            go.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
            Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityArtMaterialLibrary.ApplyVariant(go.GetComponent<Renderer>(), CityArtMaterialCategory.Wood, color, 0.75f);
            var wander = go.AddComponent<CityNpcWander>();
            wander.Configure(a, b, 0.55f + (category.Length % 5) * 0.08f);
        }
    }

    public sealed class CityNpcWander : MonoBehaviour
    {
        private Vector3 _a;
        private Vector3 _b;
        private float _speed = 0.6f;
        private float _t;
        private bool _forward = true;

        public void Configure(Vector3 a, Vector3 b, float speed)
        {
            _a = a;
            _b = b;
            _speed = Mathf.Max(0.2f, speed);
            _t = Random.value;
        }

        private void Update()
        {
            _t += Time.deltaTime * _speed * (_forward ? 1f : -1f);
            if (_t >= 1f)
            {
                _t = 1f;
                _forward = false;
            }
            else if (_t <= 0f)
            {
                _t = 0f;
                _forward = true;
            }

            transform.localPosition = Vector3.Lerp(_a, _b, _t);
        }
    }
}
