using System;
using UnityEngine;
using Valgor.City.Data;

namespace Valgor.City.Buildings
{
    [RequireComponent(typeof(Collider))]
    public sealed class BuildingView : MonoBehaviour
    {
        private Renderer _renderer = null!;
        private TextMesh _label = null!;
        private GameObject? _collectableMarker;
        private Color _baseColor;
        private bool _selected;

        public event Action<BuildingView>? Clicked;
        public BuildingInstance Instance { get; private set; } = null!;

        public void Initialize(BuildingInstance instance, BuildingDefinition definition)
        {
            Instance = instance;
            name = definition.DisplayName;
            _renderer = GetComponent<Renderer>();
            _baseColor = ColorFor(instance.State);
            _renderer.material.color = _baseColor;
            _label = CreateLabel(definition.DisplayName);
            _collectableMarker = CreateCollectableMarker();
            SetCollectable(false);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyColor();
            transform.localScale = selected ? Vector3.one * 1.12f : Vector3.one;
        }

        public void SetCollectable(bool collectable)
        {
            if (_collectableMarker != null)
            {
                _collectableMarker.SetActive(collectable);
            }
        }

        public void RefreshStateColor()
        {
            _baseColor = ColorFor(Instance.State);
            ApplyColor();
        }

        private void ApplyColor()
        {
            _renderer.material.color = _selected ? Color.Lerp(_baseColor, Color.white, 0.45f) : _baseColor;
        }

        private void OnMouseUpAsButton() => Clicked?.Invoke(this);

        private TextMesh CreateLabel(string displayName)
        {
            var labelObject = new GameObject("Name");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * 1.25f;
            labelObject.transform.rotation = Quaternion.Euler(55f, 45f, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = displayName;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.18f;
            label.fontSize = 48;
            label.color = Color.white;
            return label;
        }

        private GameObject CreateCollectableMarker()
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "CollectableMarker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = Vector3.up * 2.1f;
            marker.transform.localScale = Vector3.one * 0.35f;
            marker.GetComponent<Collider>().enabled = false;
            marker.GetComponent<Renderer>().material.color = new Color(0.2f, 0.95f, 0.35f);
            return marker;
        }

        private static Color ColorFor(BuildingState state) => state switch
        {
            BuildingState.Ready => new Color(0.18f, 0.55f, 0.75f),
            BuildingState.Available => new Color(0.88f, 0.66f, 0.2f),
            BuildingState.Locked => new Color(0.28f, 0.3f, 0.35f),
            _ => new Color(0.55f, 0.3f, 0.7f)
        };
    }
}
