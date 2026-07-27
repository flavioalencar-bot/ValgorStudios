using System;
using UnityEngine;
using Valgor.City.Visual;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Nodes
{
    /// <summary>
    /// Disco de território — escala do disc não contamina labels.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class RegionNodeView : MonoBehaviour
    {
        private Renderer _discRenderer = null!;
        private Color _baseColor;
        private Vector3 _discBaseScale;
        private Transform _disc = null!;
        private TextMesh _label = null!;
        private bool _selected;

        public event Action<RegionNodeView>? Clicked;
        public RegionInstance Instance { get; private set; } = null!;

        public void Initialize(RegionInstance instance, RegionDefinition definition)
        {
            Instance = instance;
            name = "Region_" + definition.DisplayName;

            _disc = transform.Find("Disc") ?? CreateDisc();
            _discRenderer = _disc.GetComponent<Renderer>();
            _baseColor = ColorFor(instance.Status);
            CityVisualMaterials.Apply(_discRenderer, new Color(_baseColor.r, _baseColor.g, _baseColor.b, 1f));
            _discBaseScale = _disc.localScale;
            _label = CreateLabel(definition.DisplayName);
            _label.gameObject.SetActive(false);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            var color = selected ? Color.Lerp(_baseColor, Color.white, 0.3f) : _baseColor;
            CityVisualMaterials.Apply(_discRenderer, color);
            _disc.localScale = selected ? _discBaseScale * 1.06f : _discBaseScale;
            if (_label != null)
            {
                _label.gameObject.SetActive(selected);
            }
        }

        private Transform CreateDisc()
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Disc";
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = Vector3.up * 0.04f;
            disc.transform.localScale = new Vector3(3.4f, 0.04f, 3.4f);
            Destroy(disc.GetComponent<Collider>());
            return disc.transform;
        }

        private void OnMouseUpAsButton() => Clicked?.Invoke(this);

        private TextMesh CreateLabel(string text)
        {
            // Label no root (escala 1) — nunca filho do cilindro achatado.
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * 0.35f;
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.characterSize = 0.06f;
            label.fontSize = 42;
            label.color = new Color(0.95f, 0.93f, 0.85f);
            return label;
        }

        private static Color ColorFor(RegionStatus status) => status switch
        {
            RegionStatus.Available => new Color(0.28f, 0.52f, 0.34f),
            RegionStatus.Cleared => new Color(0.28f, 0.42f, 0.62f),
            _ => new Color(0.32f, 0.34f, 0.36f)
        };
    }
}
