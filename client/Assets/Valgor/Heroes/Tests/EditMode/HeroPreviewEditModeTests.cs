using NUnit.Framework;
using UnityEngine;
using Valgor.Heroes.Data;
using Valgor.Heroes.Preview360;

namespace Valgor.Heroes.Tests
{
    public sealed class HeroPreviewEditModeTests
    {
        [Test]
        public void Faction_Colors_Match_Provisional_Palette()
        {
            Assert.AreEqual(new Color(0.55f, 0.08f, 0.12f), HeroPreviewFactionColors.ForFaction(HeroFaction.RosaDeSangue));
            Assert.AreEqual(new Color(0.18f, 0.42f, 0.86f), HeroPreviewFactionColors.ForFaction(HeroFaction.AsasDoAmanhecer));
            Assert.AreEqual(new Color(0.86f, 0.70f, 0.18f), HeroPreviewFactionColors.ForFaction(HeroFaction.GuardaDaOrdem));
        }

        [Test]
        public void Humanoid_Dummy_Uses_Heroic_Adult_Proportions()
        {
            var material = HumanoidDummyFactory.CreateUrpCompatibleMaterial(Color.white);
            Assume.That(material, Is.Not.Null);

            var dummy = HumanoidDummyFactory.Create(null, material);
            var renderers = dummy.GetComponentsInChildren<MeshRenderer>();
            Assert.GreaterOrEqual(renderers.Length, 7);

            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach (var r in renderers)
            {
                var b = r.bounds;
                minY = Mathf.Min(minY, b.min.y);
                maxY = Mathf.Max(maxY, b.max.y);
            }

            var height = maxY - minY;
            Assert.That(height, Is.EqualTo(HumanoidDummyFactory.TargetHeightMeters).Within(0.12f));
            Assert.That(minY, Is.EqualTo(0f).Within(0.08f));

            // Longer-leg read: crotch/hips should sit near ~half body (not high dwarf crotch)
            var hips = dummy.transform.Find("Hips");
            Assert.IsNotNull(hips);
            Assert.That(hips.localPosition.y / HumanoidDummyFactory.TargetHeightMeters, Is.EqualTo(0.49f).Within(0.08f));

            Object.DestroyImmediate(dummy);
            Object.DestroyImmediate(material);
        }

        [Test]
        public void Humanoid_Dummy_Has_Visible_Scale_And_Renderers()
        {
            var material = HumanoidDummyFactory.CreateUrpCompatibleMaterial(Color.white);
            Assume.That(material, Is.Not.Null);

            var dummy = HumanoidDummyFactory.Create(null, material);
            Assert.IsNotNull(dummy);
            Assert.Greater(dummy.transform.localScale.sqrMagnitude, 0.01f);

            var renderers = dummy.GetComponentsInChildren<MeshRenderer>();
            Assert.GreaterOrEqual(renderers.Length, 5);

            foreach (var renderer in renderers)
            {
                Assert.IsNotNull(renderer.sharedMaterial);
                Assert.Greater(renderer.transform.lossyScale.magnitude, 0.01f);
            }

            Object.DestroyImmediate(dummy);
            Object.DestroyImmediate(material);
        }

        [Test]
        public void Preview_Controller_Frames_Camera_On_Dummy()
        {
            var go = new GameObject("PreviewTest");
            var preview = go.AddComponent<HeroPreviewController>();
            preview.ShowHero("HERO_VORTEX_000", HeroFaction.GuardaDaOrdem);

            Assert.IsNotNull(preview.PreviewCamera);
            Assert.IsNotNull(preview.PreviewTexture);
            Assert.IsNotNull(preview.CurrentDummy);
            Assert.AreEqual(preview.PreviewCamera.targetTexture, preview.PreviewTexture);
            Assert.Greater(preview.PreviewCamera.cullingMask, 0);

            var toDummy = preview.CurrentDummy.transform.position - preview.PreviewCamera.transform.position;
            Assert.Greater(Vector3.Dot(preview.PreviewCamera.transform.forward, toDummy.normalized), 0.5f);

            Object.DestroyImmediate(go);
        }
    }
}
