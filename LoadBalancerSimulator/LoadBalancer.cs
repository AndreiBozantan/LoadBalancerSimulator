using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LoadBalancerSimulator.Internal.Selectors;

namespace LoadBalancerSimulator
{
    public class LoadBalancer : IServiceProvider
    {
        private readonly ConcurrentDictionary<string, StatefulProvider> providers = new ConcurrentDictionary<string, StatefulProvider>();
        private readonly Selector<string> selector;
        private readonly Task heartbeatCheckTask;
        private int concurrentRequestCount;

        public enum ProviderSelectorType
        {
            Random,
            RoundRobin
        }

        public LoadBalancer(int maxProvidersCount, ProviderSelectorType pst, TimeSpan heartbeatInterval)
        {
            Id = Guid.NewGuid().ToString();
            MaxProvidersCount = maxProvidersCount;
            ConcurrentRequestCount = 0;
            selector = Selector<string>.Create(pst);
            heartbeatCheckTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(heartbeatInterval);
                    await HeartbeatCheck();
                }
            });
        }

        public int MaxParallelRequestsPerProvider { get; set; } = 2;

        public int MaxProvidersCount { get; }

        public int ConcurrentRequestCount { get => concurrentRequestCount; private set => concurrentRequestCount = value; }

        public int ProvidersInServiceCount { get; private set; }

        public int TotalProvidersCount { get => this.providers.Count; }

        public string Id { get; }

        public void SetProviderExcluded(IServiceProvider p, bool excluded)
        {
            lock (selector)
            {
                if (!providers.TryGetValue(p.Id, out var provider))
                {
                    throw new ArgumentException($"Provider with id '{p.Id}' is not registered.");
                }
                provider.Excluded = excluded;
                UpdateSelectorValues();
            }
        }

        public bool IsProviderExcluded(IServiceProvider p)
        {
            if (!providers.TryGetValue(p.Id, out var provider))
            {
                throw new ArgumentException($"Provider with id '{p.Id}' is not registered.");
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
            lock (selector)
            {
                var maxCount = MaxProvidersCount - this.providers.Count;
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
        }

        public async Task<string> Get()
        {
            StatefulProvider? provider = null;
            lock (selector)
            {
                if (ProvidersInServiceCount == 0)
                {
                    throw new InvalidOperationException("No alive providers.");
                }
                var id = selector.Select();
                if (!providers.TryGetValue(id, out provider))
                {
                    throw new KeyNotFoundException($"Provider with id {id} is not registered.");
                }
                // interlocked is used here because the final decrement is outside the lock
                // (since we do not lock the balancer while waiting for the response from a provider)
                Interlocked.Increment(ref concurrentRequestCount);
                var maxConcurrentRequests = ProvidersInServiceCount * MaxParallelRequestsPerProvider;
                if (ConcurrentRequestCount > maxConcurrentRequests)
                {
                    Interlocked.Decrement(ref concurrentRequestCount);
                    throw new Exception("Maximum cluster capacity exceeded.");
                }
            }
            try
            {
                return await provider.Get();
            }
            finally
            {
                Interlocked.Decrement(ref concurrentRequestCount);
            }
        }

        public Task<bool> Check()
        {
            return Task.FromResult(true);
        }

        public void DisplayStatus(System.IO.TextWriter tw, bool displayProviders)
        {
            tw.Write("Load Balancer status: ");
            tw.Write($" requests:  {ConcurrentRequestCount} / {ProvidersInServiceCount * MaxParallelRequestsPerProvider};");
            tw.WriteLine($" providers: {ProvidersInServiceCount} / {TotalProvidersCount}");
            if (displayProviders)
            {
                foreach (var kv in providers)
                {
                    var p = kv.Value;
                    var status = p.InService ? "ON " : "off";
                    System.Console.WriteLine($"   provider: {p.Id}; status: {status}; HeartBeats: {p.SuccessfulConsecutiveChecks}; excluded: {p.Excluded}");
                }
            }
        }

        private void UpdateSelectorValues()
        {
            var ids = providers.ToArray().Where(kv => kv.Value.InService).Select(kv => kv.Key);
            selector.UpdateValues(ids);
            ProvidersInServiceCount = ids.Count();
        }

        private async Task HeartbeatCheck()
        {
            var checkTasks = providers.Values.Select(async p =>
            {
                var status = await p.Check();
            });
            await Task.WhenAll(checkTasks);
            lock (selector)
            {
                UpdateSelectorValues();
            }
        }
    }
}
