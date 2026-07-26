using UnityEngine;
using Valgor.Bootstrap;
using Valgor.WorldMap.Simulation;

namespace Valgor.WorldMap.Simulation
{
    /// <summary>
    /// Host DDOL que dispara o tick global da simulação mundial a cada frame.
    /// </summary>
    public sealed class GlobalMarchTickHost : MonoBehaviour
    {
        private GlobalMarchTickService? _service;

        public void Bind(GlobalMarchTickService service) =>
            _service = service ?? throw new System.ArgumentNullException(nameof(service));

        private void Update() => _service?.Tick();

        public static GlobalMarchTickHost EnsureHost(GlobalMarchTickService service)
        {
            if (GameBootstrap.Services != null &&
                GameBootstrap.Services.TryGet<GlobalMarchTickHost>(out var existing))
            {
                existing.Bind(service);
                return existing;
            }

            var hostObject = new GameObject(nameof(GlobalMarchTickHost));
            DontDestroyOnLoad(hostObject);
            var host = hostObject.AddComponent<GlobalMarchTickHost>();
            host.Bind(service);
            GameBootstrap.Services?.Register(host);
            return host;
        }
    }
}
