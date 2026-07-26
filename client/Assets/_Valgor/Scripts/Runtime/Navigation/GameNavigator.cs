using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.Scenes;

namespace Valgor.Navigation
{
    /// <summary>
    /// Navegação entre estados/cenas principais do jogo (Beta Técnica 0.1).
    /// </summary>
    public sealed class GameNavigator
    {
        private readonly ServiceRegistry _services;
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;

        public GameNavigator(ServiceRegistry services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _stateMachine = services.Get<GameStateMachine>();
            _sceneLoader = services.Get<SceneLoader>();
        }

        public GameState CurrentState => _stateMachine.Current;

        public IEnumerator GoToMainMenu()
        {
            ExitOptionalModules();
            yield return _sceneLoader.LoadAsync(SceneIds.MainMenu, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.MainMenu);
        }

        public IEnumerator GoToCity()
        {
            ExitWorldAndHeroesModules();
            yield return _sceneLoader.LoadAsync(SceneIds.City, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.PlayerCity);

            if (_services.TryGet<IPlayerCityModule>(out var city))
            {
                city.Enter();
            }
        }

        /// <summary>Alias de <see cref="GoToCity"/>.</summary>
        public IEnumerator GoToPlayerCity() => GoToCity();

        public IEnumerator GoToWorldMap()
        {
            ExitCityAndHeroesModules();
            yield return _sceneLoader.LoadAsync(SceneIds.WorldMap, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.WorldMap);

            if (_services.TryGet<IWorldMapModule>(out var world))
            {
                world.Enter();
            }
        }

        public IEnumerator GoToHeroes()
        {
            if (_services.TryGet<IPlayerCityModule>(out var city) && city.IsLoaded)
            {
                city.Exit();
            }

            if (_services.TryGet<IWorldMapModule>(out var world) && world.IsLoaded)
            {
                world.Exit();
            }

            yield return _sceneLoader.LoadAsync(SceneIds.Heroes, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.Heroes);
        }

        public IEnumerator GoToDragonTower()
        {
            BetaFocusHints.RequestDragonTower();
            yield return GoToCity();
        }

        private void ExitOptionalModules()
        {
            ExitCityAndHeroesModules();
            if (_services.TryGet<IWorldMapModule>(out var world) && world.IsLoaded)
            {
                world.Exit();
            }
        }

        private void ExitCityAndHeroesModules()
        {
            if (_services.TryGet<IPlayerCityModule>(out var city) && city.IsLoaded)
            {
                city.Exit();
            }
        }

        private void ExitWorldAndHeroesModules()
        {
            if (_services.TryGet<IWorldMapModule>(out var world) && world.IsLoaded)
            {
                world.Exit();
            }
        }
    }
}
