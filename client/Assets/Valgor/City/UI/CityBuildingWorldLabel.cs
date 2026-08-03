using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Buildings;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Marcador de label mundo — sem collider, não bloqueia seleção.</summary>
    public sealed class CityBuildingWorldLabel : MonoBehaviour
    {
        private BuildingView _view;
        private TextMesh _mesh;
        private bool _forceVisible;
        private bool _distanceVisible;

        public BuildingView View => _view;
        public string DefinitionId => _view != null && _view.Instance != null
            ? _view.Instance.DefinitionId
            : string.Empty;

        public void Bind(BuildingView view, TextMesh mesh)
        {
            _view = view;
            _mesh = mesh;
            Apply();
        }

        public void SetForceVisible(bool visible)
        {
            _forceVisible = visible;
            Apply();
        }

        public void SetDistanceVisible(bool visible)
        {
            _distanceVisible = visible;
            Apply();
        }

        public void SetText(string text)
        {
            if (_mesh != null)
            {
                _mesh.text = text;
            }
        }

        private void Apply()
        {
            if (_mesh == null)
            {
                return;
            }

            _mesh.gameObject.SetActive(_forceVisible || _distanceVisible);
        }
    }

    /// <summary>Pooling/culling de labels dos 17 edifícios oficiais.</summary>
    public sealed class CityBuildingLabelManager : MonoBehaviour
    {
        private readonly List<CityBuildingWorldLabel> _labels = new(24);
        private float _nextTick;
        private UnityEngine.Camera _cam;

        public int ActiveOfficialCount
        {
            get
            {
                var n = 0;
                for (var i = 0; i < _labels.Count; i++)
                {
                    var id = _labels[i].DefinitionId;
                    if (!string.IsNullOrEmpty(id) && BuildingIdMigration.IsCanonical(id))
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        public static CityBuildingLabelManager Ensure()
        {
            var existing = FindFirstObjectByType<CityBuildingLabelManager>();
            if (existing != null)
            {
                return existing;
            }

            var host = new GameObject(nameof(CityBuildingLabelManager));
            return host.AddComponent<CityBuildingLabelManager>();
        }

        public void Register(BuildingView view, TextMesh mesh)
        {
            if (view == null || mesh == null)
            {
                return;
            }

            var world = mesh.GetComponent<CityBuildingWorldLabel>() ??
                        mesh.gameObject.AddComponent<CityBuildingWorldLabel>();
            world.Bind(view, mesh);
            if (!_labels.Contains(world))
            {
                _labels.Add(world);
            }
        }

        public void SetSelected(BuildingView view)
        {
            for (var i = 0; i < _labels.Count; i++)
            {
                var lab = _labels[i];
                lab.SetForceVisible(view != null && lab.View == view);
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextTick)
            {
                return;
            }

            _nextTick = Time.unscaledTime + 0.2f;
            if (_cam == null)
            {
                var ctrl = FindFirstObjectByType<Valgor.City.Camera.CityCameraController>();
                _cam = ctrl != null ? ctrl.GetComponent<UnityEngine.Camera>() : UnityEngine.Camera.main;
            }

            if (_cam == null)
            {
                return;
            }

            var ortho = _cam.orthographic ? _cam.orthographicSize : 14f;
            var show = ortho <= ValgorHudLayoutConfig.BuildingLabelMaxOrtho &&
                       ortho >= ValgorHudLayoutConfig.BuildingLabelMinOrtho;

            if (ortho > ValgorHudLayoutConfig.BuildingLabelMaxOrtho)
            {
                show = false;
            }

            for (var i = 0; i < _labels.Count; i++)
            {
                _labels[i].SetDistanceVisible(show);
            }
        }
    }
}
