using Valgor.Addressables;
using Valgor.Audio;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.Localization;
using Valgor.Navigation;
using Valgor.Scenes;

namespace Valgor.Bootstrap
{
    /// <summary>
    /// Entrada do cliente. Registra serviços, inicia sessão e executa o LoadingFlow.
    /// </summary>
    public sealed class GameBootstrap : UnityEngine.MonoBehaviour
    {
        public static ValgorGame Game { get; private set; }
        public static ServiceRegistry Services { get; private set; }

        private void Awake()
        {
            if (Services != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Services = BuildRegistry();
            Game = new ValgorGame(Services);
            Valgor.UI.BetaNavigationBar.Ensure();
        }

        private System.Collections.IEnumerator Start()
        {
            var loadingFlow = new LoadingFlow(Services);
            yield return loadingFlow.Run();
        }

        private static ServiceRegistry BuildRegistry()
        {
            var registry = new ServiceRegistry();

            var session = new GameSession();
            var stateMachine = new GameStateMachine();
            var sceneLoader = new SceneLoader();
            var addressables = new AddressablesService();
            var audio = GetOrCreatePersistent<AudioManager>();
            var localization = GetOrCreatePersistent<LocalizationBootstrap>();

            registry.Register(session);
            registry.Register(stateMachine);
            registry.Register(sceneLoader);
            registry.Register(addressables);
            registry.Register(audio);
            registry.Register(localization);
            registry.Register(registry);

            var navigator = new GameNavigator(registry);
            registry.Register(navigator);
            registry.Register<IHeroesGateway>(new ProvisionalHeroesGateway());
            var dragons = new ProvisionalDragonGateway();
            registry.Register<IDragonModule>(dragons);
            registry.Register<IDragonGateway>(dragons);

            return registry;
        }

        private static T GetOrCreatePersistent<T>() where T : UnityEngine.MonoBehaviour
        {
            var existing = FindFirstObjectByType<T>();
            if (existing != null)
            {
                DontDestroyOnLoad(existing.gameObject);
                return existing;
            }

            var host = new UnityEngine.GameObject(typeof(T).Name);
            DontDestroyOnLoad(host);
            return host.AddComponent<T>();
        }
    }
}
