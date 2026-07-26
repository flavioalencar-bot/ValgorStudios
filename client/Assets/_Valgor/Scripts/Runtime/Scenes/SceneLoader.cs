using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valgor.Scenes
{
    public sealed class SceneLoader
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public event Action<float> ProgressChanged;
        public event Action<string> SceneLoaded;

        public bool IsLoaded(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        public IEnumerator LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("Scene name is required.", nameof(sceneName));
            }

            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to start loading scene '{sceneName}'.");
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                ProgressChanged?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            ProgressChanged?.Invoke(1f);
            operation.allowSceneActivation = true;
            yield return operation;

            SceneLoaded?.Invoke(sceneName);
        }

        public IEnumerator UnloadAsync(string sceneName)
        {
            if (!IsLoaded(sceneName))
            {
                yield break;
            }

            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null)
            {
                yield break;
            }

            yield return operation;
        }
    }
}
