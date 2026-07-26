using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valgor.Scenes
{
    public sealed class SceneLoader
    {
        public event Action<float> ProgressChanged;

        public IEnumerator LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                ProgressChanged?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            ProgressChanged?.Invoke(1f);
            operation.allowSceneActivation = true;
            yield return operation;
        }
    }
}
