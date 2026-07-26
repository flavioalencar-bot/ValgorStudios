using System;
using System.Collections.Generic;

namespace Valgor.Core
{
    /// <summary>
    /// Composition root do cliente. Serviços são registrados no bootstrap e resolvidos pelos módulos.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<TService>(TService instance) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _services[typeof(TService)] = instance;
        }

        public bool TryGet<TService>(out TService service) where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var boxed) && boxed is TService typed)
            {
                service = typed;
                return true;
            }

            service = null!;
            return false;
        }

        public TService Get<TService>() where TService : class
        {
            if (TryGet<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service not registered: {typeof(TService).FullName}");
        }

        public bool IsRegistered<TService>() where TService : class
        {
            return _services.ContainsKey(typeof(TService));
        }
    }
}
