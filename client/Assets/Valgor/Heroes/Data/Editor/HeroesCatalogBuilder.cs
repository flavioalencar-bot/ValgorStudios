#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;

namespace Valgor.Heroes.EditorTools
{
    public static class HeroesCatalogBuilder
    {
        private const string OutputRoot = "Assets/Valgor/Heroes/Data/Generated";

        private readonly struct HeroSeedRow
        {
            public HeroSeedRow(
                string id,
                string name,
                string title,
                HeroFaction faction,
                HeroRarity rarity,
                string className,
                string powerName,
                float durationSec,
                float cooldownSec)
            {
                Id = id;
                Name = name;
                Title = title;
                Faction = faction;
                Rarity = rarity;
                ClassName = className;
                PowerName = powerName;
                DurationSec = durationSec;
                CooldownSec = cooldownSec;
            }

            public string Id { get; }
            public string Name { get; }
            public string Title { get; }
            public HeroFaction Faction { get; }
            public HeroRarity Rarity { get; }
            public string ClassName { get; }
            public string PowerName { get; }
            public float DurationSec { get; }
            public float CooldownSec { get; }
        }

        // Values mirrored from docs/game-design/heroes/heroes.seed.json — do not invent.
        private static readonly HeroSeedRow[] Rows =
        {
            new("HERO_VORTEX_000", "Vortex", "O Rei dos Dragões", HeroFaction.GuardaDaOrdem, HeroRarity.Mitica, "Comandante Dracônico", "Domínio do Rei", 10f, 60f),
            new("HERO_ELYRA_001", "Elyra", "A Caçadora Esmeralda", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Arqueira", "Olho da Caçadora", 10f, 35f),
            new("HERO_CONSORTE_002", "A definir", "A Consorte de Valgor", HeroFaction.GuardaDaOrdem, HeroRarity.Mitica, "Lanceira Real", "Voto Eterno", 10f, 45f),
            new("HERO_SOMBRA_003", "A definir", "A Arqueira da Sombra", HeroFaction.RosaDeSangue, HeroRarity.Lendaria, "Atiradora Élfica", "Domínio Sombrio", 8f, 42f),
            new("HERO_LYRIANNE_004", "Lyrianne", "A Sentinela de Prata", HeroFaction.GuardaDaOrdem, HeroRarity.Lendaria, "Sentinela", "Julgamento Prateado", 9f, 44f),
            new("HERO_AKEMI_005", "Akemi", "A Lâmina Celeste", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Duelista", "Dança das Lâminas", 8f, 40f),
            new("HERO_SERENA_006", "Serena Rubra", "A Caçadora Carmesim", HeroFaction.RosaDeSangue, HeroRarity.Lendaria, "Atiradora", "Coração Carmesim", 9f, 42f),
            new("HERO_ABISMO_007", "A definir", "A Maga do Abismo", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Maga", "Domínio do Vazio", 7f, 50f),
            new("HERO_ZAHARA_008", "Zahara", "A Guardiã dos Círculos", HeroFaction.GuardaDaOrdem, HeroRarity.Lendaria, "Mística", "Órbita Real", 10f, 46f),
            new("HERO_NYXARA_009", "Nyxara", "A Guardiã das Sombras", HeroFaction.RosaDeSangue, HeroRarity.Lendaria, "Executora", "Juízo Noturno", 8f, 43f),
            new("HERO_VESPERA_010", "Vespera", "A Dama do Leque", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Estrategista", "Suspiro Final", 11f, 47f)
        };

        [MenuItem("Valgor/Heroes/Rebuild Catalog From Seed")]
        public static void RebuildFromSeed()
        {
            var seedPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/game-design/heroes/heroes.seed.json"));
            if (!File.Exists(seedPath))
            {
                Debug.LogWarning($"Seed de referência não encontrado em {seedPath}. Gerando a partir da tabela espelhada.");
            }

            Directory.CreateDirectory(OutputRoot);

            if (AssetDatabase.LoadAssetAtPath<FactionConfigSO>($"{OutputRoot}/FactionConfig.asset") == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FactionConfigSO>(), $"{OutputRoot}/FactionConfig.asset");
            }

            var catalogPath = $"{OutputRoot}/HeroCatalog.asset";
            var existing = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(catalogPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(catalogPath);
            }

            var catalog = ScriptableObject.CreateInstance<HeroCatalogSO>();
            catalog.Version = "1.0.0";
            AssetDatabase.CreateAsset(catalog, catalogPath);

            foreach (var row in Rows)
            {
                CreateHero(catalog, row);
            }

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Catálogo de heróis regenerado ({Rows.Length}) em {OutputRoot}");
        }

        private static void CreateHero(HeroCatalogSO catalog, HeroSeedRow row)
        {
            var hero = ScriptableObject.CreateInstance<HeroDefinitionSO>();
            hero.name = row.Id;
            hero.Id = row.Id;
            hero.DisplayName = row.Name;
            hero.Title = row.Title;
            hero.Faction = row.Faction;
            hero.Rarity = row.Rarity;
            hero.ClassName = row.ClassName;
            hero.DefaultSkinId = $"SKIN_{row.Id}_DEFAULT";
            hero.PrefabAddress = $"heroes/{row.Id}/prefab";
            hero.PortraitAddress = $"heroes/{row.Id}/portrait";

            var power = ScriptableObject.CreateInstance<SpecialPowerDefinitionSO>();
            power.name = $"POWER_{row.Id}";
            power.Id = $"POWER_{row.Id}";
            power.HeroId = row.Id;
            power.DisplayName = row.PowerName;
            power.ActiveDurationSec = row.DurationSec;
            power.CooldownSec = row.CooldownSec;
            power.VfxAddress = $"vfx/special/{power.Id}";
            power.SfxAddress = $"sfx/special/{power.Id}";
            hero.SpecialPower = power;

            AssetDatabase.AddObjectToAsset(hero, catalog);
            AssetDatabase.AddObjectToAsset(power, catalog);
            catalog.Heroes.Add(hero);
        }
    }
}
#endif
