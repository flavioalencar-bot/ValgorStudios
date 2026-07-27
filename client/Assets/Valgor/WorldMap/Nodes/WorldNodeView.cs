using UnityEngine;
using Valgor.City.Visual;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Visual;

namespace Valgor.WorldMap.Nodes
{
    [RequireComponent(typeof(Collider))]
    public sealed class WorldNodeView : MonoBehaviour
    {
        private Renderer[] _renderers = System.Array.Empty<Renderer>();
        private Color[] _identityColors = System.Array.Empty<Color>();
        private Transform? _visual;
        private TextMesh _label = null!;
        private GameObject? _levelBadge;
        private float _labelHeight = 1.8f;
        private bool _selected;

        public event System.Action<WorldNodeView>? Clicked;
        public WorldNodeInstance Instance { get; private set; } = null!;
        public WorldMapNodeDefinition Definition { get; private set; } = null!;

        public void Initialize(WorldNodeInstance instance, WorldMapNodeDefinition definition, float labelHeight = 1.8f)
        {
            Instance = instance;
            Definition = definition;
            name = definition.DisplayName;
            _labelHeight = labelHeight;
            _visual = transform.Find("Visual");
            CacheRenderers();
            RefreshVisual();
            _label = CreateLabel(FormatLabel(definition));
            _label.gameObject.SetActive(false);
            _levelBadge = CreateLevelBadge(definition);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            for (var i = 0; i < _renderers.Length; i++)
            {
                var color = selected
                    ? Color.Lerp(_identityColors[i], Color.white, 0.35f)
                    : _identityColors[i];
                CityVisualMaterials.Apply(_renderers[i], color);
            }

            if (_visual != null)
            {
                _visual.localScale = selected ? Vector3.one * 1.1f : Vector3.one;
            }

            if (_label != null)
            {
                _label.gameObject.SetActive(selected);
                if (selected)
                {
                    _label.text = FormatLabel(Definition);
                }
            }
        }

        public void RefreshVisual()
        {
            var statusColor = WorldNodeMeshFactory.ColorFor(Definition.Kind, Instance.Status);
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (Instance.Status is WorldNodeStatus.Locked or WorldNodeStatus.Depleted)
                {
                    _identityColors[i] = statusColor;
                }

                CityVisualMaterials.Apply(_renderers[i], _selected
                    ? Color.Lerp(_identityColors[i], Color.white, 0.35f)
                    : _identityColors[i]);
            }
        }

        private static string FormatLabel(WorldMapNodeDefinition definition)
        {
            if (definition is WorldResourceNode resource)
            {
                return $"{definition.DisplayName}\nNv.{resource.Level}";
            }

            if (definition is WorldCreatureNode creature)
            {
                return $"{definition.DisplayName}\nAmeaça {creature.ThreatLevel}";
            }

            return definition.DisplayName;
        }

        private void CacheRenderers()
        {
            _renderers = _visual != null
                ? _visual.GetComponentsInChildren<Renderer>()
                : GetComponentsInChildren<Renderer>();
            _identityColors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                _identityColors[i] = ReadColor(_renderers[i]);
            }
        }

        private void OnMouseUpAsButton() => Clicked?.Invoke(this);

        private TextMesh CreateLabel(string text)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * (_labelHeight + 0.35f);
            // Top-down map: flat on XZ, readable from above.
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "LabelPlate";
            plate.transform.SetParent(labelObject.transform, false);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localScale = new Vector3(2.2f, 0.9f, 0.05f);
            Destroy(plate.GetComponent<Collider>());
            CityVisualMaterials.Apply(plate.GetComponent<Renderer>(), new Color(0.08f, 0.1f, 0.12f, 0.92f));

            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.045f;
            label.fontSize = 48;
            label.color = new Color(0.95f, 0.92f, 0.82f);
            return label;
        }

        private GameObject CreateLevelBadge(WorldMapNodeDefinition definition)
        {
            var level = definition switch
            {
                WorldResourceNode resource => resource.Level,
                WorldCreatureNode creature => creature.ThreatLevel,
                _ => 0
            };

            if (level <= 0)
            {
                return null!;
            }

            var badgeRoot = new GameObject("LevelBadge");
            badgeRoot.transform.SetParent(transform, false);
            badgeRoot.transform.localPosition = Vector3.up * 0.12f;

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Disc";
            disc.transform.SetParent(badgeRoot.transform, false);
            disc.transform.localScale = new Vector3(0.7f, 0.04f, 0.7f);
            Destroy(disc.GetComponent<Collider>());
            var color = definition is WorldResourceNode
                ? new Color(0.35f, 0.7f, 0.4f)
                : new Color(0.8f, 0.32f, 0.28f);
            CityVisualMaterials.Apply(disc.GetComponent<Renderer>(), color);

            var textObject = new GameObject("LevelText");
            textObject.transform.SetParent(badgeRoot.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            textObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var text = textObject.AddComponent<TextMesh>();
            text.text = level.ToString();
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = 0.05f;
            text.fontSize = 48;
            text.color = Color.white;
            return badgeRoot;
        }

        private static Color ReadColor(Renderer renderer)
        {
            var material = renderer.sharedMaterial;
            if (material == null) return Color.gray;
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return material.color;
        }
    }
}
