using System;
using UnityEngine;
using Valgor.City.Visual;
using Valgor.WorldMap.Marches;

namespace Valgor.WorldMap.Visual
{
    /// <summary>
    /// Exército/marcha visível no mapa: interpola origem → alvo → retorno.
    /// </summary>
    public sealed class MarchArmyView : MonoBehaviour
    {
        private Transform _body = null!;
        private Transform? _dragonFollower;
        private Renderer[] _renderers = null!;
        private Color[] _baseColors = Array.Empty<Color>();
        private TextMesh _label = null!;
        private LineRenderer? _path;
        private TrailRenderer? _trail;

        public static MarchArmyView Create(Transform parent)
        {
            var go = new GameObject("MarchArmy");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<MarchArmyView>();
            view.BuildVisual();
            go.SetActive(false);
            return view;
        }

        public void Sync(
            MarchOrder? march,
            System.Func<string, Vector3> nodeWorldPosition,
            System.DateTime utcNow,
            string? formationHint = null,
            bool showDragonCompanion = false,
            string? dragonStageLabel = null,
            bool dragonMounted = false)
        {
            if (march == null ||
                march.State is MarchState.Completed or MarchState.Cancelled or MarchState.Preparing)
            {
                gameObject.SetActive(false);
                if (_path != null)
                {
                    _path.enabled = false;
                }

                SetDragonCompanionVisible(false);
                return;
            }

            gameObject.SetActive(true);
            var origin = nodeWorldPosition(march.OriginNodeId);
            var target = nodeWorldPosition(march.TargetNodeId);
            var position = ResolvePosition(march, origin, target, utcNow);
            transform.position = position;

            var look = march.State == MarchState.Returning ? origin - target : target - origin;
            if (look.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            }

            _label.text = LabelFor(march, formationHint, showDragonCompanion, dragonStageLabel, dragonMounted);
            ApplyTint(ColorFor(march.State));
            SyncPath(march, origin, target, position);
            SetDragonCompanionVisible(showDragonCompanion);
        }

        private void SetDragonCompanionVisible(bool show)
        {
            if (show)
            {
                EnsureDragonFollower();
            }

            if (_dragonFollower != null)
            {
                _dragonFollower.gameObject.SetActive(show);
            }
        }

        private void EnsureDragonFollower()
        {
            if (_dragonFollower != null)
            {
                return;
            }

            var root = new GameObject("DragonCompanion").transform;
            root.SetParent(transform, false);
            root.localPosition = new Vector3(-0.95f, 0.55f, -0.85f);
            root.localScale = Vector3.one * 0.55f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "DragonBody";
            body.transform.SetParent(root, false);
            body.transform.localPosition = Vector3.up * 0.35f;
            body.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
            Destroy(body.GetComponent<Collider>());
            CityVisualMaterials.Apply(body.GetComponent<Renderer>(), new Color(0.72f, 0.32f, 0.14f));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "DragonHead";
            head.transform.SetParent(root, false);
            head.transform.localPosition = new Vector3(0f, 0.85f, 0.25f);
            head.transform.localScale = Vector3.one * 0.4f;
            Destroy(head.GetComponent<Collider>());
            CityVisualMaterials.Apply(head.GetComponent<Renderer>(), new Color(0.85f, 0.45f, 0.18f));

            var wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingL.name = "WingL";
            wingL.transform.SetParent(root, false);
            wingL.transform.localPosition = new Vector3(-0.45f, 0.55f, 0f);
            wingL.transform.localScale = new Vector3(0.08f, 0.35f, 0.55f);
            wingL.transform.localRotation = Quaternion.Euler(10f, -25f, 0f);
            Destroy(wingL.GetComponent<Collider>());
            CityVisualMaterials.Apply(wingL.GetComponent<Renderer>(), new Color(0.55f, 0.22f, 0.1f));

            var wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingR.name = "WingR";
            wingR.transform.SetParent(root, false);
            wingR.transform.localPosition = new Vector3(0.45f, 0.55f, 0f);
            wingR.transform.localScale = new Vector3(0.08f, 0.35f, 0.55f);
            wingR.transform.localRotation = Quaternion.Euler(10f, 25f, 0f);
            Destroy(wingR.GetComponent<Collider>());
            CityVisualMaterials.Apply(wingR.GetComponent<Renderer>(), new Color(0.55f, 0.22f, 0.1f));

            _dragonFollower = root;
        }

        private void SyncPath(MarchOrder march, Vector3 origin, Vector3 target, Vector3 current)
        {
            if (_path == null)
            {
                return;
            }

            _path.enabled = true;
            var home = origin + Vector3.up * 0.4f;
            var dest = target + Vector3.up * 0.4f;
            var here = current;
            if (march.State == MarchState.Returning)
            {
                _path.positionCount = 3;
                _path.SetPosition(0, dest);
                _path.SetPosition(1, here);
                _path.SetPosition(2, home);
            }
            else if (march.State is MarchState.Arrived or MarchState.Gathering)
            {
                _path.positionCount = 2;
                _path.SetPosition(0, home);
                _path.SetPosition(1, dest);
            }
            else
            {
                _path.positionCount = 3;
                _path.SetPosition(0, home);
                _path.SetPosition(1, here);
                _path.SetPosition(2, dest);
            }
        }

        private void BuildVisual()
        {
            _body = new GameObject("Body").transform;
            _body.SetParent(transform, false);

            CreatePart(PrimitiveType.Capsule, new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 0.7f, 0.7f), new Color(0.85f, 0.72f, 0.35f));
            CreatePart(PrimitiveType.Cube, new Vector3(0f, 0.55f, 0.55f), new Vector3(0.9f, 0.35f, 1.2f), new Color(0.35f, 0.4f, 0.55f));
            CreatePart(PrimitiveType.Sphere, new Vector3(0f, 1.55f, 0f), Vector3.one * 0.45f, new Color(0.9f, 0.55f, 0.2f));

            _renderers = _body.GetComponentsInChildren<Renderer>();
            _baseColors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                _baseColors[i] = ReadColor(_renderers[i]);
            }

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * 1.6f;
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _label = labelObject.AddComponent<TextMesh>();
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.characterSize = 0.05f;
            _label.fontSize = 42;
            _label.color = Color.white;
            _label.text = "Marcha";
            labelObject.SetActive(false);

            _trail = gameObject.AddComponent<TrailRenderer>();
            _trail.time = 1.4f;
            _trail.startWidth = 0.45f;
            _trail.endWidth = 0.05f;
            var shader = Shader.Find("Valgor/Heroes/DummyUnlit")
                         ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _trail.material = new Material(shader);
                _trail.startColor = new Color(1f, 0.85f, 0.35f, 0.85f);
                _trail.endColor = new Color(1f, 0.85f, 0.35f, 0f);
            }
            else
            {
                Destroy(_trail);
                _trail = null;
            }

            _path = gameObject.AddComponent<LineRenderer>();
            _path.positionCount = 0;
            _path.widthMultiplier = 0.22f;
            _path.numCapVertices = 2;
            _path.textureMode = LineTextureMode.Tile;
            if (shader != null)
            {
                _path.material = new Material(shader);
                _path.startColor = new Color(1f, 0.9f, 0.45f, 0.9f);
                _path.endColor = new Color(1f, 0.55f, 0.2f, 0.55f);
            }

            _path.enabled = false;
        }

        private void CreatePart(PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(_body, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }

        private void ApplyTint(Color tint)
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                CityVisualMaterials.Apply(_renderers[i], Color.Lerp(_baseColors[i], tint, 0.45f));
            }
        }

        private static Vector3 ResolvePosition(MarchOrder march, Vector3 origin, Vector3 target, System.DateTime utcNow)
        {
            switch (march.State)
            {
                case MarchState.Marching:
                {
                    var duration = (march.ArrivalAt - march.DepartureAt).TotalSeconds;
                    if (duration <= 0.01) return target;
                    var t = Mathf.Clamp01((float)((utcNow - march.DepartureAt).TotalSeconds / duration));
                    return Vector3.Lerp(origin, target, t) + Vector3.up * 1.1f;
                }
                case MarchState.Arrived:
                case MarchState.Gathering:
                    return target + Vector3.up * 1.1f;
                case MarchState.Returning:
                {
                    if (!march.ReturnAt.HasValue) return origin + Vector3.up * 1.1f;
                    var outbound = System.Math.Max(0.01, (march.ArrivalAt - march.DepartureAt).TotalSeconds);
                    var returnStart = march.ReturnAt.Value.AddSeconds(-outbound);
                    var t = Mathf.Clamp01((float)((utcNow - returnStart).TotalSeconds / outbound));
                    return Vector3.Lerp(target, origin, t) + Vector3.up * 1.1f;
                }
                default:
                    return origin + Vector3.up * 1.1f;
            }
        }

        private static string LabelFor(
            MarchOrder march,
            string? formationHint,
            bool showDragon,
            string? dragonStageLabel,
            bool dragonMounted)
        {
            var prefix = string.IsNullOrEmpty(formationHint) ? string.Empty : formationHint + " · ";
            var dragon = string.Empty;
            if (showDragon)
            {
                var stage = string.IsNullOrEmpty(dragonStageLabel) ? "Dragão" : dragonStageLabel;
                dragon = dragonMounted ? $" · Dragão {stage} (montaria)" : $" · Dragão {stage}";
            }

            var load = march.ResourceLoad > 0 ? $" · carga {march.ResourceLoad}" : string.Empty;
            return march.State switch
            {
                MarchState.Marching => prefix + "Em marcha" + dragon + load,
                MarchState.Arrived => prefix + "Chegou" + dragon + load,
                MarchState.Gathering => prefix + "Coletando" + dragon + load,
                MarchState.Returning => prefix + "Retornando" + dragon + load,
                _ => prefix + "Marcha" + dragon + load
            };
        }

        private static Color ColorFor(MarchState state) => state switch
        {
            MarchState.Gathering => new Color(0.3f, 0.9f, 0.4f),
            MarchState.Returning => new Color(0.95f, 0.75f, 0.25f),
            MarchState.Arrived => new Color(0.4f, 0.7f, 1f),
            _ => new Color(0.95f, 0.7f, 0.25f)
        };

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
