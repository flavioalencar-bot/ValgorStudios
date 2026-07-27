using System.Collections;
using UnityEngine;
using Valgor.Core;

namespace Valgor.Scenes
{
    /// <summary>
    /// Boot: sessão → Loading (splash ≤3s + loading) → MainMenu.
    /// </summary>
    public sealed class LoadingFlow
    {
        private static readonly string[] SplashMessages =
        {
            "Preparando o reino...",
            "Despertando os dragões...",
            "Reunindo os heróis...",
            "Fortificando a cidade..."
        };

        private readonly ServiceRegistry _services;
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly GameSession _session;
        private LoadingScreenController _loadingScreen;

        public LoadingFlow(ServiceRegistry services)
        {
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            _stateMachine = services.Get<GameStateMachine>();
            _sceneLoader = services.Get<SceneLoader>();
            _session = services.Get<GameSession>();
        }

        public event System.Action<float, string> ProgressChanged;

        public IEnumerator Run()
        {
            BetaPlayerSettings.ApplyRuntime();
            _stateMachine.TransitionTo(GameState.Bootstrapping);
            _session.Begin();
            LocalPlayerProfile.ApplyToSession(_session);
            Report(0.05f, SplashMessages[0]);

            yield return InitializeLocalization();
            Report(0.12f, SplashMessages[0]);

            _stateMachine.TransitionTo(GameState.Loading);
            var splashStart = Time.realtimeSinceStartup;
            yield return LoadSceneWithProgress(SceneIds.Loading, 0.12f, 0.35f);
            _loadingScreen = Object.FindFirstObjectByType<LoadingScreenController>();
            _loadingScreen?.SetBrandMessage(SplashMessages[0]);

            // Brand/splash ≤ 3 segundos.
            const float splashBudget = 2.8f;
            var slice = splashBudget / SplashMessages.Length;
            for (var i = 0; i < SplashMessages.Length; i++)
            {
                var t = 0.35f + (0.45f * (i + 1) / SplashMessages.Length);
                _loadingScreen?.SetBrandMessage(SplashMessages[i]);
                Report(t, SplashMessages[i]);
                yield return new WaitForSecondsRealtime(slice);
            }

            Report(0.85f, SplashMessages[^1]);
            yield return LoadSceneWithProgress(SceneIds.MainMenu, 0.85f, 0.98f);
            _loadingScreen = null;

            var elapsed = Time.realtimeSinceStartup - splashStart;
            if (elapsed < splashBudget)
            {
                yield return new WaitForSecondsRealtime(splashBudget - elapsed);
            }

            _stateMachine.TransitionTo(GameState.MainMenu);
            Report(1f, "Pronto");
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
                var msg = SplashMessages[Mathf.Clamp(Mathf.FloorToInt(value * SplashMessages.Length), 0, SplashMessages.Length - 1)];
                Report(Mathf.Lerp(from, to, value), msg);
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
            _loadingScreen?.SetProgress(clamped, stage);
        }
    }
}
