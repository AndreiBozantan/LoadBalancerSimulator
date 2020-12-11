using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using LoadBalancerSimulator.Internal.Selectors;

namespace LoadBalancerSimulator
{
    public class LoadBalancer : IServiceProvider
    {
        private ConcurrentDictionary<string, IServiceProvider> providers = new ConcurrentDictionary<string, IServiceProvider>();
        private Selector<string>? selector = null;
        private ProviderSelectorType providerSelectorType;

        public enum ProviderSelectorType
        {
            Random,
            RoundRobin
        }

        public LoadBalancer(int capacity, ProviderSelectorType pst)
        {
            Id = Guid.NewGuid().ToString();
            Capacity = capacity;
            providerSelectorType = pst;
        }

        public int Capacity { get; }

        public int ProvidersCount { get => this.providers.Count; }

        public string Id { get; }

        /// <summary>
        /// Registers a list of providers, up to the load balancer capacity.
        /// Providers with duplicated id will be replaced with the new version.
        /// </summary>
        /// <param name="newProviders">The new providers to be added.</param>
        /// <returns>The number of added providers</returns>
        public int Register(IEnumerable<IServiceProvider> newProviders)
        {
            var maxCount = Capacity - this.providers.Count;
            var count = 0;
            foreach (var p in newProviders)
            {
                if (count == maxCount)
                {
                    break;
                }
                providers[p.Id] = p;
                count++;
            }
            selector = Selector<string>.Create(providerSelectorType, providers.Keys);
            return count;
        }

        public async Task<string> Get()
        {
            if (selector == null)
            {
                throw new InvalidOperationException("No providers registered.");
            }
            var id = selector.Select();
            if (!providers.TryGetValue(id, out var provider))
            {
                throw new KeyNotFoundException($"Provider with id {id} was removed.");
            }
            return await provider.Get();
        }

    }
}
