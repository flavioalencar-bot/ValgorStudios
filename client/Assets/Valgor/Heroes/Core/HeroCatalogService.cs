using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Valgor.Heroes.Data;

namespace Valgor.Heroes.Core
{
    [Serializable]
    public sealed class HeroCatalogApiResponse
    {
        public string version;
        public List<HeroApiDto> heroes;
    }

    [Serializable]
    public sealed class HeroApiDto
    {
        public string id;
        public string name;
        public string title;
        public string displayName;
        public string rarity;
        public string faction;
        public string @class;
        public string role;
        public string position;
        public string weapon;
        public string element;
        public string status;
        public string defaultSkinId;
        public string prefabAddress;
        public string portraitAddress;
        public SpecialPowerApiDto specialPower;
    }

    [Serializable]
    public sealed class SpecialPowerApiDto
    {
        public string id;
        public string name;
        public float activeDurationSec;
        public float cooldownSec;
        public List<string> effects;
    }

    public sealed class HeroCatalogService
    {
        private readonly string _apiBaseUrl;
        private HeroCatalogSO _localCatalog;
        private HeroCatalogApiResponse _remoteCatalog;

        public HeroCatalogService(string apiBaseUrl = "http://localhost:5100")
        {
            _apiBaseUrl = apiBaseUrl.TrimEnd('/');
        }

        public void BindLocalCatalog(HeroCatalogSO catalog) => _localCatalog = catalog;

        public IReadOnlyList<HeroDefinitionSO> LocalHeroes =>
            _localCatalog != null ? _localCatalog.Heroes : Array.Empty<HeroDefinitionSO>();

        public async Task<HeroCatalogApiResponse> FetchRemoteCatalogAsync()
        {
            using var request = UnityWebRequest.Get($"{_apiBaseUrl}/api/heroes/catalog");
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Falha ao carregar catálogo: {request.error}");
            }

            _remoteCatalog = JsonUtility.FromJson<HeroCatalogApiResponse>(request.downloadHandler.text);
            return _remoteCatalog;
        }

        public HeroDefinitionSO FindLocal(string heroId)
        {
            if (_localCatalog == null) return null;
            foreach (var hero in _localCatalog.Heroes)
            {
                if (hero != null && hero.Id == heroId) return hero;
            }

            return null;
        }
    }

    public sealed class HeroRuntimeController : MonoBehaviour
    {
        [SerializeField] private HeroDefinitionSO definition;
        [SerializeField] private SpecialPowers.SpecialPowerController specialPowerController;
        [SerializeField] private Skins.HeroSkinController skinController;

        public HeroDefinitionSO Definition => definition;

        public void Bind(HeroDefinitionSO hero)
        {
            definition = hero;
        }
    }

    public sealed class HeroTeamBuilder
    {
        private readonly List<string> _heroIds = new();

        public IReadOnlyList<string> HeroIds => _heroIds;

        public bool TryAdd(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return false;
            if (_heroIds.Contains(heroId)) return false;
            if (_heroIds.Count >= 5) return false;
            _heroIds.Add(heroId);
            return true;
        }

        public void Clear() => _heroIds.Clear();
    }
}
