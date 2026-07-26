#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Valgor.Heroes.Data;
using Valgor.Heroes.Factions;

namespace Valgor.Heroes.EditorTools
{
    public static class HeroesCatalogBuilder
    {
        public const string OutputRoot = "Assets/Valgor/Heroes/Data/Generated";

        private readonly struct HeroSeedRow
        {
            public HeroSeedRow(
                string id,
                string name,
                string title,
                HeroFaction faction,
                HeroRarity rarity,
                string className,
                string role,
                string position,
                string weapon,
                string element,
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
                Role = role;
                Position = position;
                Weapon = weapon;
                Element = element;
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
            public string Role { get; }
            public string Position { get; }
            public string Weapon { get; }
            public string Element { get; }
            public string PowerName { get; }
            public float DurationSec { get; }
            public float CooldownSec { get; }
        }

        // Mirrored from docs/game-design/heroes/heroes.seed.json — do not invent or alter.
        private static readonly HeroSeedRow[] Rows =
        {
            new("HERO_VORTEX_000", "Vortex", "O Rei dos Dragões", HeroFaction.GuardaDaOrdem, HeroRarity.Mitica, "Comandante Dracônico", "Liderança / Dano / Controle", "Linha de frente", "Espada dracônica e vínculo com dragão", "Chama Dracônica", "Domínio do Rei", 10f, 60f),
            new("HERO_ELYRA_001", "Elyra", "A Caçadora Esmeralda", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Arqueira", "Dano de longo alcance", "Retaguarda", "Arco longo recurvo esmeralda", "Natureza", "Olho da Caçadora", 10f, 35f),
            new("HERO_CONSORTE_002", "A definir", "A Consorte de Valgor", HeroFaction.GuardaDaOrdem, HeroRarity.Mitica, "Lanceira Real", "Suporte / Liderança / Dano", "Linha intermediária", "Lança-cajado celestial branca e dourada", "Luz Sagrada", "Voto Eterno", 10f, 45f),
            new("HERO_SOMBRA_003", "A definir", "A Arqueira da Sombra", HeroFaction.RosaDeSangue, HeroRarity.Lendaria, "Atiradora Élfica", "Dano / Controle", "Retaguarda", "Besta élfica ornamentada", "Sombra", "Domínio Sombrio", 8f, 42f),
            new("HERO_LYRIANNE_004", "Lyrianne", "A Sentinela de Prata", HeroFaction.GuardaDaOrdem, HeroRarity.Lendaria, "Sentinela", "Precisão / Proteção", "Linha intermediária", "Arco lunar de haste longa", "Luz Lunar", "Julgamento Prateado", 9f, 44f),
            new("HERO_AKEMI_005", "Akemi", "A Lâmina Celeste", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Duelista", "Assassina de curto alcance", "Linha de frente móvel", "Lâminas gêmeas celestes", "Gelo Celeste", "Dança das Lâminas", 8f, 40f),
            new("HERO_SERENA_006", "Serena Rubra", "A Caçadora Carmesim", HeroFaction.RosaDeSangue, HeroRarity.Lendaria, "Atiradora", "Longa distância / Dano crítico", "Retaguarda", "Lançador rúnico carmesim de longo alcance", "Fogo", "Coração Carmesim", 9f, 42f),
            new("HERO_ABISMO_007", "A definir", "A Maga do Abismo", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Maga", "Dano em área / Controle / Enfraquecimento", "Retaguarda", "Cajado do Abismo com orbe violeta", "Abismo", "Domínio do Vazio", 7f, 50f),
            new("HERO_ZAHARA_008", "Zahara", "A Guardiã dos Círculos", HeroFaction.GuardaDaOrdem, HeroRarity.Lendaria, "Mística", "Controle arcano de médio alcance", "Linha intermediária", "Anéis rúnicos gêmeos", "Éter Safira", "Órbita Real", 10f, 46f),
            new("HERO_NYXARA_009", "Nyxara", "A Guardiã das Sombras", HeroFaction.RosaDeSangue, HeroRarity.Lendaria, "Executora", "Assassina de controle", "Flanco", "Correntes-lâmina gêmeas", "Sombra Rubra", "Juízo Noturno", 8f, 43f),
            new("HERO_VESPERA_010", "Vespera", "A Dama do Leque", HeroFaction.AsasDoAmanhecer, HeroRarity.Lendaria, "Estrategista", "Atiradora de médio e longo alcance", "Retaguarda", "Leque de lâminas e dardos ocultos", "Crepúsculo Violeta", "Suspiro Final", 11f, 47f)
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
            AssetDatabase.ImportAsset(catalogPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            var reloaded = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(catalogPath);
            var count = reloaded?.Heroes?.Count ?? 0;
            Debug.Log($"Catálogo de heróis regenerado ({Rows.Length}) em {OutputRoot}; reload={count}");
            if (count != Rows.Length)
            {
                Debug.LogWarning(
                    $"Reload do catálogo retornou {count} heróis. Sub-assets: " +
                    string.Join(", ", AssetDatabase.LoadAllAssetsAtPath(catalogPath).Select(a => a.name)));
            }
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
            hero.Role = row.Role;
            hero.Position = row.Position;
            hero.WeaponId = row.Weapon;
            hero.ElementId = row.Element;
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
