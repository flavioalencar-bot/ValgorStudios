using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.Scenes;

namespace Valgor.Navigation
{
    /// <summary>
    /// Navegação entre estados/cenas principais do jogo.
    /// Módulos de cidade/mundo são opcionais até serem registrados.
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

        public IEnumerator GoToMainMenu()
        {
            ExitOptionalModules();
            yield return _sceneLoader.LoadAsync(SceneIds.MainMenu, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.MainMenu);
        }

        public IEnumerator GoToPlayerCity()
        {
            yield return _sceneLoader.LoadAsync(SceneIds.PlayerCity, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.PlayerCity);

            if (_services.TryGet<IPlayerCityModule>(out var city))
            {
                city.Enter();
            }
        }

        public IEnumerator GoToWorldMap()
        {
            yield return _sceneLoader.LoadAsync(SceneIds.WorldMap, LoadSceneMode.Single);
            _stateMachine.TransitionTo(GameState.WorldMap);

            if (_services.TryGet<IWorldMapModule>(out var world))
            {
                world.Enter();
            }
        }

        private void ExitOptionalModules()
        {
            if (_services.TryGet<IPlayerCityModule>(out var city) && city.IsLoaded)
            {
                city.Exit();
            }

            if (_services.TryGet<IWorldMapModule>(out var world) && world.IsLoaded)
            {
                world.Exit();
            }
        }
    }
}
