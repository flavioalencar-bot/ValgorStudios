using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.Characters
{
    public static class HeroAddressableLoader
    {
        public static async Task<GameObject> LoadPrefabAsync(string addressableKey)
        {
            if (string.IsNullOrWhiteSpace(addressableKey))
            {
                return null;
            }

            var settingsPath = Path.Combine(Application.streamingAssetsPath, "aa", "settings.json");
            if (!File.Exists(settingsPath))
            {
                Debug.LogWarning(
                    $"[Valgor.Heroes] Addressables indisponível — fallback silencioso para '{addressableKey}'.");
                return null;
            }

            try
            {
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }

                Debug.LogWarning($"[Valgor.Heroes] Chave Addressable ausente: '{addressableKey}'.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Valgor.Heroes] Addressables load failed for '{addressableKey}': {ex.Message}");
            }

            return null;
        }

        public static void Release(GameObject asset)
        {
            if (asset == null)
            {
                return;
            }

            try
            {
                UnityEngine.AddressableAssets.Addressables.Release(asset);
            }
            catch
            {
                /* ignore */
            }
        }

        public static string KeyForHero(string heroId) =>
            heroId == VortexAssetPaths.HeroId
                ? VortexAssetPaths.AddressablePrefabKey
                : $"heroes/{heroId}/prefab";
    }
}
