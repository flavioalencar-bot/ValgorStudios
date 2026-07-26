using System.Collections;
using UnityEngine;
using Valgor.Addressables;
using Valgor.Audio;
using Valgor.Core;
using Valgor.Localization;
using Valgor.Scenes;

namespace Valgor.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static ValgorGame Game { get; private set; }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);

            var sceneLoader = new SceneLoader();
            var audio = GetOrCreate<AudioManager>();
            var localization = GetOrCreate<LocalizationBootstrap>();
            var addressables = new AddressablesService();
            Game = new ValgorGame(sceneLoader, audio, addressables, localization);

            yield return localization.Initialize();
            yield return sceneLoader.LoadAsync("Loading");
            yield return sceneLoader.LoadAsync("MainMenu");
        }

        private static T GetOrCreate<T>() where T : MonoBehaviour
        {
            var service = FindFirstObjectByType<T>();
            if (service != null)
                return service;

            var serviceObject = new GameObject(typeof(T).Name);
            return serviceObject.AddComponent<T>();
        }
    }
}
