using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Valgor.ContentLoading
{
    /// <summary>
    /// Carrega Addressables com fallback silencioso quando o catálogo não existe.
    /// Erros vão só para o log — nunca devem abrir console de jogador.
    /// </summary>
    public sealed class AddressablesService
    {
        private bool? _catalogAvailable;

        public bool IsCatalogAvailable()
        {
            if (_catalogAvailable.HasValue)
            {
                return _catalogAvailable.Value;
            }

            var settingsPath = Path.Combine(Application.streamingAssetsPath, "aa", "settings.json");
            _catalogAvailable = File.Exists(settingsPath);
            if (!_catalogAvailable.Value)
            {
                Debug.LogWarning(
                    "[Valgor.Addressables] Catálogo ausente (StreamingAssets/aa/settings.json). " +
                    "Loads retornam fallback sem inicializar Addressables.");
            }

            return _catalogAvailable.Value;
        }

        public AsyncOperationHandle<T> LoadAsset<T>(object key)
        {
            if (!IsCatalogAvailable())
            {
                return default;
            }

            try
            {
                return Addressables.LoadAssetAsync<T>(key);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Valgor.Addressables] Falha ao carregar '{key}': {ex.Message}");
                return default;
            }
        }

        public IEnumerator LoadAssetAsync<T>(object key, System.Action<T> completed)
        {
            if (!IsCatalogAvailable())
            {
                completed?.Invoke(default);
                yield break;
            }

            AsyncOperationHandle<T> handle;
            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Valgor.Addressables] Falha ao carregar '{key}': {ex.Message}");
                completed?.Invoke(default);
                yield break;
            }

            yield return handle;
            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
            {
                completed?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogWarning($"[Valgor.Addressables] Chave ausente ou inválida: '{key}'.");
                completed?.Invoke(default);
            }
        }

        public void Release<T>(AsyncOperationHandle<T> handle)
        {
            try
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Valgor.Addressables] Release falhou: {ex.Message}");
            }
        }
    }
}
