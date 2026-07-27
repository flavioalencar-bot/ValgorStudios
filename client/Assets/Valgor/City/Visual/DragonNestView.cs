using System.Collections.Generic;
using UnityEngine;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.Dragons.Data;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Placeholders 3D dos ocupantes do ninho na Torre dos Dragões.
    /// </summary>
    public sealed class DragonNestView : MonoBehaviour
    {
        private Transform _occupantsRoot = null!;
        private DragonService? _dragons;
        private readonly List<GameObject> _spawned = new();

        public void Bind(DragonService dragons)
        {
            if (_dragons != null)
            {
                _dragons.Changed -= OnDragonsChanged;
            }

            _dragons = dragons;
            _occupantsRoot = transform.Find("Visual/NestOccupants") ?? transform;
            _dragons.Changed += OnDragonsChanged;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_dragons != null)
            {
                _dragons.Changed -= OnDragonsChanged;
            }
        }

        private void OnDragonsChanged(object? sender, DragonChangedEvent e) => Refresh();

        public void Refresh()
        {
            foreach (var go in _spawned)
            {
                if (go != null) Destroy(go);
            }

            _spawned.Clear();
            if (_dragons == null || _occupantsRoot == null) return;

            var statuses = _dragons.GetDragonStatuses();
            var index = 0;
            foreach (var status in statuses)
            {
                if (index >= 4) break;
                var angle = index * (Mathf.PI * 0.5f);
                var offset = new Vector3(Mathf.Cos(angle) * 0.7f, 0f, Mathf.Sin(angle) * 0.7f);
                _spawned.Add(CreateOccupant(status, offset));
                index++;
            }
        }

        private GameObject CreateOccupant(DragonStatusInfo status, Vector3 localOffset)
        {
            var isEgg = status.StateLabel.IndexOf("EGG", System.StringComparison.OrdinalIgnoreCase) >= 0
                        || status.GrowthStageLabel.IndexOf("EGG", System.StringComparison.OrdinalIgnoreCase) >= 0;

            var go = GameObject.CreatePrimitive(isEgg ? PrimitiveType.Sphere : PrimitiveType.Capsule);
            go.name = "Nest_" + status.DisplayName;
            go.transform.SetParent(_occupantsRoot, false);
            go.transform.localPosition = localOffset + Vector3.up * (isEgg ? 0.25f : 0.55f);
            go.transform.localScale = isEgg ? new Vector3(0.55f, 0.7f, 0.55f) : new Vector3(0.45f, 0.55f, 0.45f);
            Destroy(go.GetComponent<Collider>());

            var color = isEgg
                ? new Color(0.85f, 0.72f, 0.45f)
                : new Color(0.62f, 0.28f, 0.14f); // brasas / cinzas — sem roxo/magenta
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
            return go;
        }
    }
}
