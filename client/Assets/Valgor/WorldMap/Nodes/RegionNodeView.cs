using System;
using UnityEngine;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Nodes
{
    [RequireComponent(typeof(Collider))]
    public sealed class RegionNodeView : MonoBehaviour
    {
        private Renderer _renderer = null!;
        private Color _baseColor;
        private Vector3 _baseScale;

        public event Action<RegionNodeView>? Clicked;
        public RegionInstance Instance { get; private set; } = null!;

        public void Initialize(RegionInstance instance, RegionDefinition definition)
        {
            Instance = instance;
            name = "Region_" + definition.DisplayName;
            _renderer = GetComponent<Renderer>();
            _baseColor = ColorFor(instance.Status);
            _baseColor.a = 0.35f;
            _renderer.material.color = _baseColor;
            _baseScale = transform.localScale;
            CreateLabel(definition.DisplayName);
        }

        public void SetSelected(bool selected)
        {
            _renderer.material.color = selected ? Color.Lerp(_baseColor, Color.white, 0.35f) : _baseColor;
            transform.localScale = selected ? _baseScale * 1.08f : _baseScale;
        }

        private void OnMouseUpAsButton() => Clicked?.Invoke(this);

        private TextMesh CreateLabel(string text)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * 1.4f;
            labelObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.characterSize = 0.2f;
            label.fontSize = 42;
            label.color = Color.white;
            return label;
        }

        private static Color ColorFor(RegionStatus status) => status switch
        {
            RegionStatus.Available => new Color(0.25f, 0.7f, 0.4f),
            RegionStatus.Cleared => new Color(0.2f, 0.45f, 0.85f),
            _ => new Color(0.3f, 0.32f, 0.36f)
        };
    }
}
