using System;
using UnityEngine;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Nodes
{
    [RequireComponent(typeof(Collider))]
    public sealed class WorldNodeView : MonoBehaviour
    {
        private Renderer _renderer = null!;
        private Color _baseColor;
        private Vector3 _baseScale;

        public event Action<WorldNodeView>? Clicked;
        public WorldNodeInstance Instance { get; private set; } = null!;
        public WorldMapNodeDefinition Definition { get; private set; } = null!;

        public void Initialize(WorldNodeInstance instance, WorldMapNodeDefinition definition)
        {
            Instance = instance;
            Definition = definition;
            name = definition.DisplayName;
            _renderer = GetComponent<Renderer>();
            _baseColor = ColorFor(definition.Kind, instance.Status);
            _renderer.material.color = _baseColor;
            _baseScale = transform.localScale;
            CreateLabel(definition.DisplayName);
        }

        public void SetSelected(bool selected)
        {
            _renderer.material.color = selected ? Color.Lerp(_baseColor, Color.white, 0.45f) : _baseColor;
            transform.localScale = selected ? _baseScale * 1.25f : _baseScale;
        }

        public void RefreshVisual()
        {
            _baseColor = ColorFor(Definition.Kind, Instance.Status);
            _renderer.material.color = _baseColor;
        }

        private void OnMouseUpAsButton() => Clicked?.Invoke(this);

        private void CreateLabel(string text)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * 1.5f;
            labelObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.characterSize = 0.18f;
            label.fontSize = 40;
            label.color = Color.white;
        }

        private static Color ColorFor(WorldNodeKind kind, WorldNodeStatus status)
        {
            if (status == WorldNodeStatus.Locked)
            {
                return new Color(0.28f, 0.3f, 0.32f);
            }

            if (status == WorldNodeStatus.Depleted)
            {
                return new Color(0.45f, 0.4f, 0.28f);
            }

            return kind switch
            {
                WorldNodeKind.City => new Color(0.2f, 0.55f, 0.9f),
                WorldNodeKind.Village => new Color(0.35f, 0.75f, 0.45f),
                WorldNodeKind.Resource => new Color(0.85f, 0.7f, 0.2f),
                WorldNodeKind.Creature => new Color(0.85f, 0.35f, 0.25f),
                WorldNodeKind.Dragon => new Color(0.65f, 0.25f, 0.8f),
                WorldNodeKind.Landmark => new Color(0.55f, 0.65f, 0.75f),
                _ => Color.gray
            };
        }
    }
}
