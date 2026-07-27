using NUnit.Framework;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;
using UnityEngine;

namespace Valgor.Heroes.Tests
{
    public sealed class FactionRulesEditModeTests
    {
        [Test]
        public void Rosa_Beats_Guarda()
        {
            var resolver = new FactionAdvantageResolver(CreateConfig());
            Assert.IsTrue(resolver.HasAdvantage(HeroFactionIds.RosaDeSangue, HeroFactionIds.GuardaDaOrdem));
            Assert.AreEqual(1.15f, resolver.ResolveDamageMultiplier(HeroFactionIds.RosaDeSangue, HeroFactionIds.GuardaDaOrdem));
        }

        [Test]
        public void Guarda_Beats_Asas()
        {
            var resolver = new FactionAdvantageResolver(CreateConfig());
            Assert.IsTrue(resolver.HasAdvantage(HeroFactionIds.GuardaDaOrdem, HeroFactionIds.AsasDoAmanhecer));
        }

        [Test]
        public void Asas_Beats_Rosa()
        {
            var resolver = new FactionAdvantageResolver(CreateConfig());
            Assert.IsTrue(resolver.HasAdvantage(HeroFactionIds.AsasDoAmanhecer, HeroFactionIds.RosaDeSangue));
        }

        [Test]
        public void Team_Bonuses_Match_Seed()
        {
            var calculator = new FactionBonusCalculator(CreateConfig());
            Assert.AreEqual(1.05f, calculator.Calculate(new[]
            {
                HeroFactionIds.RosaDeSangue, HeroFactionIds.RosaDeSangue, HeroFactionIds.RosaDeSangue
            }).TotalTroopAttackMultiplier);

            Assert.AreEqual(1.07f, calculator.Calculate(new[]
            {
                HeroFactionIds.RosaDeSangue, HeroFactionIds.RosaDeSangue, HeroFactionIds.RosaDeSangue,
                HeroFactionIds.AsasDoAmanhecer, HeroFactionIds.AsasDoAmanhecer
            }).TotalTroopAttackMultiplier);

            Assert.AreEqual(1.10f, calculator.Calculate(new[]
            {
                HeroFactionIds.GuardaDaOrdem, HeroFactionIds.GuardaDaOrdem,
                HeroFactionIds.GuardaDaOrdem, HeroFactionIds.GuardaDaOrdem
            }).TotalTroopAttackMultiplier);

            Assert.AreEqual(1.15f, calculator.Calculate(new[]
            {
                HeroFactionIds.AsasDoAmanhecer, HeroFactionIds.AsasDoAmanhecer, HeroFactionIds.AsasDoAmanhecer,
                HeroFactionIds.AsasDoAmanhecer, HeroFactionIds.AsasDoAmanhecer
            }).TotalTroopAttackMultiplier);
        }

        [Test]
        public void Pending_Name_Uses_Title()
        {
            var pending = ScriptableObject.CreateInstance<HeroDefinitionSO>();
            pending.Id = "HERO_CONSORTE_002";
            pending.DisplayName = "A definir";
            pending.Title = "A Consorte de Valgor";
            Assert.AreEqual("A Consorte de Valgor", pending.ResolveDisplayName());
            Object.DestroyImmediate(pending);

            var named = ScriptableObject.CreateInstance<HeroDefinitionSO>();
            named.Id = "HERO_LYRA_TEST";
            named.DisplayName = "Lyra";
            named.Title = "A Consorte de Valgor";
            Assert.AreEqual("Lyra", named.ResolveDisplayName());
            Object.DestroyImmediate(named);
        }

        private static FactionConfigSO CreateConfig()
        {
            return ScriptableObject.CreateInstance<FactionConfigSO>();
        }
    }
}
