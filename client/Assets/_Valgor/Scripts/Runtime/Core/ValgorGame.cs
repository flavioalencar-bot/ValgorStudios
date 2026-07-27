using Valgor.ContentLoading;
using Valgor.Audio;
using Valgor.Localization;
using Valgor.Navigation;
using Valgor.Scenes;

namespace Valgor.Core
{
    /// <summary>
    /// Fachada de acesso aos serviços centrais do cliente após o bootstrap.
    /// </summary>
    public sealed class ValgorGame
    {
        public ValgorGame(ServiceRegistry services)
        {
            Services = services;
            Session = services.Get<GameSession>();
            StateMachine = services.Get<GameStateMachine>();
            SceneLoader = services.Get<SceneLoader>();
            Navigator = services.Get<GameNavigator>();
            Audio = services.Get<AudioManager>();
            Addressables = services.Get<AddressablesService>();
            Localization = services.Get<LocalizationBootstrap>();
        }

        public ServiceRegistry Services { get; }
        public GameSession Session { get; }
        public GameStateMachine StateMachine { get; }
        public SceneLoader SceneLoader { get; }
        public GameNavigator Navigator { get; }
        public AudioManager Audio { get; }
        public AddressablesService Addressables { get; }
        public LocalizationBootstrap Localization { get; }
    }
}
