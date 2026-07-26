using System;
using System.Collections;
using UnityEngine;
using Valgor.Core;

namespace Valgor.Scenes
{
    /// <summary>
    /// Orquestra o carregamento inicial: sessão → Loading → MainMenu.
    /// </summary>
    public sealed class LoadingFlow
    {
        private readonly ServiceRegistry _services;
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly GameSession _session;
        private LoadingScreenController _loadingScreen;

        public LoadingFlow(ServiceRegistry services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _stateMachine = services.Get<GameStateMachine>();
            _sceneLoader = services.Get<SceneLoader>();
            _session = services.Get<GameSession>();
        }

        public event Action<float, string> ProgressChanged;

        public IEnumerator Run()
        {
            _stateMachine.TransitionTo(GameState.Bootstrapping);
            _session.Begin();
            Report(0.05f, "session");

            yield return InitializeLocalization();
            Report(0.2f, "localization");

            _stateMachine.TransitionTo(GameState.Loading);
            yield return LoadSceneWithProgress(SceneIds.Loading, 0.2f, 0.55f);
            _loadingScreen = UnityEngine.Object.FindFirstObjectByType<LoadingScreenController>();

            Report(0.6f, "systems");
            yield return null;

            yield return LoadSceneWithProgress(SceneIds.MainMenu, 0.6f, 0.95f);

            _stateMachine.TransitionTo(GameState.MainMenu);
            Report(1f, "ready");
            _loadingScreen = null;
        }

        private IEnumerator InitializeLocalization()
        {
            if (_services.TryGet<Valgor.Localization.LocalizationBootstrap>(out var localization))
            {
                yield return localization.Initialize();
            }
        }

        private IEnumerator LoadSceneWithProgress(string sceneName, float from, float to)
        {
            void OnProgress(float value)
            {
                Report(Mathf.Lerp(from, to, value), sceneName);
            }

            _sceneLoader.ProgressChanged += OnProgress;
            try
            {
                yield return _sceneLoader.LoadAsync(sceneName);
            }
            finally
            {
                _sceneLoader.ProgressChanged -= OnProgress;
            }
        }

        private void Report(float progress, string stage)
        {
            var clamped = Mathf.Clamp01(progress);
            ProgressChanged?.Invoke(clamped, stage);
            _loadingScreen?.SetProgress(clamped);
            Debug.Log($"[Valgor.LoadingFlow] {stage} ({clamped:P0})");
        }
    }
}
