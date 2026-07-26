using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.Characters
{
    public static class HeroAddressableLoader
    {
        public static async Task<GameObject> LoadPrefabAsync(string addressableKey)
        {
            if (string.IsNullOrWhiteSpace(addressableKey)) return null;
            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    return handle.Result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Addressables load failed for '{addressableKey}': {ex.Message}");
            }

            return null;
        }

        public static void Release(GameObject asset)
        {
            if (asset == null) return;
            try { Addressables.Release(asset); }
            catch { /* ignore */ }
        }

        public static string KeyForHero(string heroId) =>
            heroId == VortexAssetPaths.HeroId
                ? VortexAssetPaths.AddressablePrefabKey
                : $"heroes/{heroId}/prefab";
    }
}
