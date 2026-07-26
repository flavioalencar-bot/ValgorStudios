using Valgor.Addressables;
using Valgor.Audio;
using Valgor.Localization;
using Valgor.Scenes;

namespace Valgor.Core
{
    public sealed class ValgorGame
    {
        public ValgorGame(
            SceneLoader sceneLoader,
            AudioManager audio,
            AddressablesService addressables,
            LocalizationBootstrap localization)
        {
            SceneLoader = sceneLoader;
            Audio = audio;
            Addressables = addressables;
            Localization = localization;
        }

        public SceneLoader SceneLoader { get; }
        public AudioManager Audio { get; }
        public AddressablesService Addressables { get; }
        public LocalizationBootstrap Localization { get; }
    }
}
