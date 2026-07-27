using System;
using UnityEngine;
using Valgor.City.Data;
using Valgor.City.Visual;

namespace Valgor.City.Buildings
{
    [RequireComponent(typeof(Collider))]
    public sealed class BuildingView : MonoBehaviour
    {
        private Renderer[] _renderers = Array.Empty<Renderer>();
        private Color[] _identityColors = Array.Empty<Color>();
        private Color[] _baseColors = Array.Empty<Color>();
        private Transform? _visual;
        private TextMesh _label = null!;
        private TextMesh? _bubbleLabel;
        private GameObject? _collectableMarker;
        private GameObject? _upgradeArrow;
        private bool _selected;
        private float _labelHeight = 3.2f;

        public event Action<BuildingView>? Clicked;
        public BuildingInstance Instance { get; private set; } = null!;

        public void Initialize(BuildingInstance instance, BuildingDefinition definition, float labelHeight = 3.2f)
        {
            Instance = instance;
            name = definition.DisplayName;
            _labelHeight = labelHeight;
            _visual = transform.Find("Visual");
            CacheRenderers();
            ApplyStateTint();
            _label = CreateLabel(FormatLabel(definition));
            _label.gameObject.SetActive(false);
            _collectableMarker = CreateCollectableMarker();
            _upgradeArrow = CreateUpgradeArrow();
            SetCollectable(0, null);
            SetUpgradeAvailable(false);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyColors();
            if (_visual != null)
            {
                _visual.localScale = selected ? Vector3.one * 1.08f : Vector3.one;
            }

            if (_label != null)
            {
                _label.gameObject.SetActive(selected);
            }
        }

        public void SetCollectable(long amount, ResourceType? resource)
        {
            var show = amount > 0;
            if (_collectableMarker != null)
            {
                _collectableMarker.SetActive(show);
            }

            if (_bubbleLabel != null)
            {
                // Sem texto técnico flutuante — só o marcador verde.
                _bubbleLabel.gameObject.SetActive(false);
            }
        }

        public void SetUpgradeAvailable(bool available)
        {
            if (_upgradeArrow != null)
            {
                _upgradeArrow.SetActive(available);
            }
        }

        public void RefreshLabel(BuildingDefinition definition)
        {
            if (_label != null)
            {
                _label.text = FormatLabel(definition);
            }
        }

        public void RefreshStateColor()
        {
            ApplyStateTint();
        }

        private string FormatLabel(BuildingDefinition definition)
        {
            if (Instance.State == BuildingState.Upgrading)
            {
                return $"{definition.DisplayName}\nNv.{Instance.Level} → {Instance.Level + 1}";
            }

            if (Instance.State == BuildingState.Available && Instance.Level <= 0)
            {
                return $"{definition.DisplayName}\nConstruir";
            }

            return $"{definition.DisplayName}\nNv.{Math.Max(0, Instance.Level)}";
        }

        private void CacheRenderers()
        {
            _renderers = _visual != null
                ? _visual.GetComponentsInChildren<Renderer>()
                : GetComponentsInChildren<Renderer>();
            _identityColors = new Color[_renderers.Length];
            _baseColors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                _identityColors[i] = ReadColor(_renderers[i]);
                _baseColors[i] = _identityColors[i];
            }
        }

        private void ApplyStateTint()
        {
            var tint = CityLayout.ToTint(Instance.State);
            for (var i = 0; i < _renderers.Length; i++)
            {
                _baseColors[i] = CityVisualMaterials.MixState(_identityColors[i], tint);
            }

            ApplyColors();
        }

        private void ApplyColors()
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                var color = _selected
                    ? Color.Lerp(_baseColors[i], Color.white, 0.35f)
                    : _baseColors[i];
                CityVisualMaterials.Apply(_renderers[i], color);
            }
        }

        private void OnMouseUpAsButton() => Clicked?.Invoke(this);

        private TextMesh CreateLabel(string displayName)
        {
            var labelObject = new GameObject("Name");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * (_labelHeight + 0.35f);
            labelObject.transform.rotation = Quaternion.Euler(30f, 45f, 0f);

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "LabelPlate";
            plate.transform.SetParent(labelObject.transform, false);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localScale = new Vector3(2.4f, 0.55f, 0.08f);
            Destroy(plate.GetComponent<Collider>());
            CityVisualMaterials.Apply(plate.GetComponent<Renderer>(), new Color(0.12f, 0.11f, 0.1f, 0.92f));

            var label = labelObject.AddComponent<TextMesh>();
            label.text = displayName;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.12f;
            label.fontSize = 42;
            label.color = new Color(0.95f, 0.9f, 0.78f);
            return label;
        }

        private GameObject CreateCollectableMarker()
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "CollectableMarker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = Vector3.up * (_labelHeight + 0.85f);
            marker.transform.localScale = Vector3.one * 0.55f;
            Destroy(marker.GetComponent<Collider>());
            CityVisualMaterials.Apply(marker.GetComponent<Renderer>(), new Color(0.25f, 0.95f, 0.4f));

            var amountObject = new GameObject("Amount");
            amountObject.transform.SetParent(marker.transform, false);
            amountObject.transform.localPosition = Vector3.up * 0.9f;
            amountObject.transform.localRotation = Quaternion.Euler(30f, 45f, 0f);
            amountObject.transform.localScale = Vector3.one * 0.35f;
            _bubbleLabel = amountObject.AddComponent<TextMesh>();
            _bubbleLabel.anchor = TextAnchor.MiddleCenter;
            _bubbleLabel.alignment = TextAlignment.Center;
            _bubbleLabel.characterSize = 0.22f;
            _bubbleLabel.fontSize = 64;
            _bubbleLabel.color = Color.white;
            _bubbleLabel.text = "0";
            return marker;
        }

        private GameObject CreateUpgradeArrow()
        {
            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arrow.name = "UpgradeArrow";
            arrow.transform.SetParent(transform, false);
            arrow.transform.localPosition = Vector3.up * (_labelHeight + 1.55f);
            arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            arrow.transform.localScale = new Vector3(0.18f, 0.28f, 0.18f);
            Destroy(arrow.GetComponent<Collider>());
            CityVisualMaterials.Apply(arrow.GetComponent<Renderer>(), new Color(0.35f, 0.95f, 0.45f));

            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Tip";
            tip.transform.SetParent(arrow.transform, false);
            tip.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            tip.transform.localScale = Vector3.one * 1.4f;
            Destroy(tip.GetComponent<Collider>());
            CityVisualMaterials.Apply(tip.GetComponent<Renderer>(), new Color(0.45f, 1f, 0.55f));
            arrow.SetActive(false);
            return arrow;
        }

        private static string ShortResource(ResourceType resource) => resource switch
        {
            ResourceType.Gold => "Ouro",
            ResourceType.Food => "Comida",
            ResourceType.Wood => "Mad",
            ResourceType.Stone => "Pedra",
            ResourceType.Iron => "Ferro",
            ResourceType.DragonEssence => "Ess",
            _ => resource.ToString()
        };

        private static string FormatAmount(long amount)
        {
            if (amount >= 1_000_000)
            {
                return (amount / 1_000_000f).ToString("0.0") + "M";
            }

            if (amount >= 1_000)
            {
                return (amount / 1_000f).ToString("0.0") + "K";
            }

            return amount.ToString();
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
