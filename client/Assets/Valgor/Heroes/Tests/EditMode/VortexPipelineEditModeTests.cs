using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Valgor.Heroes.Characters;
using Valgor.Heroes.Characters.Vortex;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Tests
{
    public sealed class VortexPipelineEditModeTests
    {
        [Test]
        public void Vortex_Paths_And_Addressable_Key_Are_Stable()
        {
            Assert.AreEqual("HERO_VORTEX_000", VortexAssetPaths.HeroId);
            Assert.AreEqual("heroes/HERO_VORTEX_000/prefab", VortexAssetPaths.AddressablePrefabKey);
            Assert.AreEqual("Assets/Valgor/Heroes/Characters/Vortex/Prefabs/Vortex_Hero.prefab", VortexAssetPaths.HeroPrefab);
            Assert.AreEqual(2.05f, VortexAssetPaths.TargetHeightMeters);
        }

        [Test]
        public void Required_Sockets_And_Animations_Match_Spec()
        {
            Assert.AreEqual(9, HeroSocketIds.Required.Length);
            Assert.Contains(HeroSocketIds.DragonLink, HeroSocketIds.Required);
            Assert.Contains(HeroAnimationIds.SpecialPower, HeroAnimationIds.Required);
            Assert.AreEqual(16, HeroAnimationIds.Required.Length);
        }

        [Test]
        public void Resolver_Falls_Back_When_Source_Missing()
        {
            var dummy = new GameObject("DummyProbe");
            var result = HeroVisualResolver.Resolve(VortexAssetPaths.HeroId, dummy);
            Assert.IsNotNull(result.Message);
            // Without built prefab in test domain, fallback to provided dummy is acceptable.
            Assert.IsTrue(result.IsTechnicalFallback || result.Prefab != null);
            Object.DestroyImmediate(dummy);
        }

        [Test]
        public void Catalog_Vortex_Keeps_Gameplay_Identity()
        {
#if UNITY_EDITOR
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(
                "Assets/Valgor/Heroes/Data/Generated/HeroCatalog.asset");
            Assert.IsNotNull(catalog);
            HeroDefinitionSO vortex = null;
            foreach (var hero in catalog.Heroes)
            {
                if (hero != null && hero.Id == VortexAssetPaths.HeroId)
                {
                    vortex = hero;
                    break;
                }
            }

            Assert.IsNotNull(vortex);
            Assert.AreEqual("Vortex", vortex.DisplayName);
            Assert.AreEqual("O Rei dos Dragões", vortex.Title);
            Assert.AreEqual(HeroFaction.GuardaDaOrdem, vortex.Faction);
            Assert.AreEqual(VortexAssetPaths.AddressablePrefabKey, vortex.PrefabAddress);
            Assert.IsNotNull(vortex.SpecialPower);
            Assert.AreEqual("Domínio do Rei", vortex.SpecialPower.DisplayName);
            Assert.AreEqual(10f, vortex.SpecialPower.ActiveDurationSec);
            Assert.AreEqual(60f, vortex.SpecialPower.CooldownSec);
#endif
        }

#if UNITY_EDITOR
        [Test]
        public void Rigged_Vortex_Source_And_Sword_Exist()
        {
            Assert.IsTrue(System.IO.File.Exists(VortexAssetPaths.Lod0), "Vortex_LOD0.fbx missing");
            Assert.IsTrue(System.IO.File.Exists(VortexAssetPaths.DragonSword), "Vortex_DragonSword.fbx missing");

            var importer = UnityEditor.AssetImporter.GetAtPath(VortexAssetPaths.Lod0) as UnityEditor.ModelImporter;
            Assert.IsNotNull(importer);
            Assert.AreEqual(UnityEditor.ModelImporterAnimationType.Human, importer.animationType);

            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(VortexAssetPaths.HeroPrefab);
            Assert.IsNotNull(prefab);
            var visual = prefab.GetComponent<HeroVisualController>();
            Assert.IsNotNull(visual);
            Assert.IsFalse(visual.UsingTechnicalFallback);

            var animator = prefab.GetComponent<Animator>();
            Assert.IsNotNull(animator);
            Assert.IsNotNull(animator.avatar);
            Assert.IsTrue(animator.avatar.isValid);

            var sword = prefab.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "Vortex_DragonSword");
            Assert.IsNotNull(sword, "Espada deve estar no prefab");
        }

        [Test]
        public void Animator_Has_Required_States_With_Motions()
        {
            var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(
                VortexAssetPaths.AnimatorController);
            Assert.IsNotNull(controller);
            var states = controller.layers[0].stateMachine.states;
            foreach (var required in HeroAnimationIds.Required)
            {
                var match = states.FirstOrDefault(s => s.state.name == required);
                Assert.IsNotNull(match.state, "Missing state " + required);
                Assert.IsNotNull(match.state.motion, "Missing motion for " + required);
            }
        }
#endif
    }
}
