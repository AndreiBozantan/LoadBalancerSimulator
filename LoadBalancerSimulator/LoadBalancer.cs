using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using LoadBalancerSimulator.Internal.Selectors;

namespace LoadBalancerSimulator
{
    public class LoadBalancer : IServiceProvider
    {
        private ConcurrentDictionary<string, StatefulProvider> providers = new ConcurrentDictionary<string, StatefulProvider>();
        private Selector<string> selector;

        public enum ProviderSelectorType
        {
            Random,
            RoundRobin
        }

        public LoadBalancer(int capacity, ProviderSelectorType pst)
        {
            Id = Guid.NewGuid().ToString();
            Capacity = capacity;
            selector = Selector<string>.Create(pst, Enumerable.Empty<string>());
        }

        public int Capacity { get; }

        public int ProvidersCount { get => this.providers.Count; }

        public string Id { get; }

        public void SetProviderExcluded(IServiceProvider p, bool excluded)
        {
            if (!providers.TryGetValue(p.Id, out var provider))
            {
                throw new ArgumentException("Provider is not registered.");
            }
            provider.Excluded = excluded;
            UpdateSelectorValues();
        }

        public bool IsProviderExcluded(IServiceProvider p)
        {
            if (!providers.TryGetValue(p.Id, out var provider))
            {
                throw new ArgumentException("Provider is not registered.");
            }
            return provider.Excluded;
        }

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
                providers[p.Id] = new StatefulProvider(p);
                count++;
            }
            UpdateSelectorValues();
            return count;
        }

        public async Task<string> Get()
        {
            if (selector == null || selector.ValuesCount == 0)
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

        private void UpdateSelectorValues()
        {
            var ids = providers.ToArray().Where(kv => !kv.Value.Excluded).Select(kv => kv.Key);
            selector.UpdateValues(ids);
        }
    }
}
