using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Valgor.Addressables
{
    public sealed class AddressablesService
    {
        public AsyncOperationHandle<T> LoadAsset<T>(object key) => Addressables.LoadAssetAsync<T>(key);

        public IEnumerator LoadAssetAsync<T>(object key, System.Action<T> completed)
        {
            var handle = LoadAsset<T>(key);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                completed?.Invoke(handle.Result);
        }

        public void Release<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
    }
}
