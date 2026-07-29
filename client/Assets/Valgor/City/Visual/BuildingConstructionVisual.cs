using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Estado visual de obra sob o edifício (andaime + poeira + debris).
    /// Filho do slot; não altera collider/lógica/escala do BuildingRoot.
    /// </summary>
    public sealed class BuildingConstructionVisual : MonoBehaviour
    {
        public const string RootName = "ConstructionVisualRoot";
        public const string ScaffoldName = "Scaffold";
        public const string DustName = "DustVFX";
        public const string DebrisName = "DebrisVFX";
        public const string AudioName = "WorkAudio";

        private Transform _root = null!;
        private Transform? _scaffoldHost;
        private ParticleSystem? _dust;
        private ParticleSystem? _debris;
        private GameObject? _scaffoldInstance;
        private string _buildingId = string.Empty;
        private bool _active;

        public bool IsActive => _active;

        public static BuildingConstructionVisual Ensure(Transform buildingRoot, string buildingDefinitionId)
        {
            var existing = buildingRoot.Find(RootName);
            BuildingConstructionVisual ctrl;
            if (existing != null)
            {
                ctrl = existing.GetComponent<BuildingConstructionVisual>() ??
                       existing.gameObject.AddComponent<BuildingConstructionVisual>();
            }
            else
            {
                var go = new GameObject(RootName);
                go.transform.SetParent(buildingRoot, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                ctrl = go.AddComponent<BuildingConstructionVisual>();
            }

            ctrl.Initialize(buildingDefinitionId);
            return ctrl;
        }

        public void Initialize(string buildingDefinitionId)
        {
            _buildingId = buildingDefinitionId ?? string.Empty;
            _root = transform;
            EnsureChildren();
            SetActive(false);
        }

        public void SetActive(bool active)
        {
            _active = active;
            if (_root != null)
            {
                _root.gameObject.SetActive(active);
            }

            if (!active)
            {
                StopFx();
                return;
            }

            EnsureScaffold();
            FitScaffoldToVisual();
            PlayFx();
        }

        private void EnsureChildren()
        {
            _scaffoldHost = EnsureChild(ScaffoldName);
            var dustGo = EnsureChild(DustName).gameObject;
            var debrisGo = EnsureChild(DebrisName).gameObject;
            EnsureChild(AudioName);

            _dust ??= CreateDust(dustGo);
            _debris ??= CreateDebris(debrisGo);

            // Audio placeholder (sem clip nesta sprint — estrutura pronta).
            var audio = EnsureChild(AudioName).gameObject.GetComponent<AudioSource>() ??
                        EnsureChild(AudioName).gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = true;
            audio.spatialBlend = 1f;
            audio.volume = 0f;
        }

        private void EnsureScaffold()
        {
            if (_scaffoldHost == null)
            {
                return;
            }

            if (_scaffoldInstance != null)
            {
                return;
            }

            var size = ConstructionScaffoldCatalog.ResolveSize(_buildingId);
            // Prefabs em Resources podem carregar magenta no player (materiais bake sem shader URP).
            // Builder runtime aplica RuntimeSafeMaterials — fonte visual desta sprint.
            _scaffoldInstance = ConstructionScaffoldBuilder.Build(size, _scaffoldHost);
            _scaffoldInstance.name = ScaffoldName + "_Runtime";
            RefreshScaffoldMaterials(_scaffoldInstance);

            _scaffoldInstance.transform.localPosition = ConstructionScaffoldCatalog.LocalOffset(_buildingId);
            _scaffoldInstance.transform.localRotation = Quaternion.identity;
            _scaffoldInstance.transform.localScale = ConstructionScaffoldCatalog.LocalScaleMultiplier(_buildingId);
            StripGameplay(_scaffoldInstance);
        }

        private static void RefreshScaffoldMaterials(GameObject scaffold)
        {
            if (scaffold == null)
            {
                return;
            }

            foreach (var renderer in scaffold.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                CityVisualMaterials.ApplySurface(renderer, CityVisualMaterials.Wood, SurfaceKind.Wood);
            }
        }

        private void FitScaffoldToVisual()
        {
            if (_scaffoldInstance == null)
            {
                return;
            }

            var visual = transform.parent != null ? transform.parent.Find("Visual") : null;
            if (visual == null)
            {
                return;
            }

            var bounds = Encapsulate(visual);
            if (bounds.size.sqrMagnitude < 0.01f)
            {
                return;
            }

            // Enquadra o andaime ao footprint sem alterar o BuildingRoot.
            var localCenter = transform.InverseTransformPoint(bounds.center);
            localCenter.y = 0f;
            var baseScale = ConstructionScaffoldCatalog.LocalScaleMultiplier(_buildingId);
            var footprint = Mathf.Max(bounds.size.x, bounds.size.z);
            var refFoot = ConstructionScaffoldCatalog.ResolveSize(_buildingId) switch
            {
                ConstructionScaffoldSize.Small => 2.6f,
                ConstructionScaffoldSize.Medium => 3.6f,
                ConstructionScaffoldSize.Large => 4.4f,
                ConstructionScaffoldSize.Castle => 6.0f,
                ConstructionScaffoldSize.Wall => 7.5f,
                _ => 3.6f
            };
            var mul = Mathf.Clamp(footprint / refFoot, 0.75f, 1.45f);
            _scaffoldInstance.transform.localPosition =
                ConstructionScaffoldCatalog.LocalOffset(_buildingId) + localCenter;
            _scaffoldInstance.transform.localScale = baseScale * mul;
        }

        private void PlayFx()
        {
            if (_dust != null && !_dust.isPlaying)
            {
                _dust.Play(true);
            }

            if (_debris != null && !_debris.isPlaying)
            {
                _debris.Play(true);
            }
        }

        private void StopFx()
        {
            if (_dust != null)
            {
                _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_debris != null)
            {
                _debris.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private Transform EnsureChild(string childName)
        {
            var t = _root.Find(childName);
            if (t != null)
            {
                return t;
            }

            var go = new GameObject(childName);
            go.transform.SetParent(_root, false);
            return go.transform;
        }

        private static ParticleSystem CreateDust(GameObject host)
        {
            var ps = host.GetComponent<ParticleSystem>() ?? host.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 2.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
            main.startColor = new Color(0.62f, 0.55f, 0.42f, 0.35f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.2f, 0.4f, 2.2f);

            var colorOver = ps.colorOverLifetime;
            colorOver.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.7f, 0.62f, 0.48f), 0f),
                    new GradientColorKey(new Color(0.55f, 0.5f, 0.4f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.4f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOver.color = grad;

            var renderer = host.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                // Material padrão do particle — evita magenta pesado.
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                             Shader.Find("Particles/Standard Unlit") ??
                             Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    renderer.sharedMaterial = new Material(shader);
                }
            }

            host.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private static ParticleSystem CreateDebris(GameObject host)
        {
            var ps = host.GetComponent<ParticleSystem>() ?? host.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 1.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
            main.startColor = new Color(0.45f, 0.4f, 0.35f, 0.55f);
            main.gravityModifier = 0.35f;
            main.maxParticles = 24;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.35f;

            host.transform.localPosition = new Vector3(0.4f, 1.2f, 0.2f);
            host.transform.localRotation = Quaternion.Euler(-70f, 20f, 0f);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private static void StripGameplay(GameObject root)
        {
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                Destroy(col);
            }

            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }

        private static Bounds Encapsulate(Transform visual)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(visual.position, Vector3.one);
            }

            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled)
                {
                    b.Encapsulate(renderers[i].bounds);
                }
            }

            return b;
        }
    }
}
