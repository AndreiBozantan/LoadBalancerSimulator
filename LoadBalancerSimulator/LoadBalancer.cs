using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancerSimulator
{
    public class LoadBalancer
    {
        private ConcurrentDictionary<string, Provider> providers = new ConcurrentDictionary<string, Provider>();
        private Selector<string>? selector = null;

        public LoadBalancer(int capacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }

        public int ProvidersCount { get => this.providers.Count; }

        /// <summary>
        /// Registers a list of providers, up to the load balancer capacity.
        /// Providers with duplicated id will be replaced with the new version.
        /// </summary>
        /// <param name="newProviders">The new providers to be added.</param>
        /// <returns>The number of added providers</returns>
        public int Register(IEnumerable<Provider> newProviders)
        {
            var maxCount = Capacity - this.providers.Count;
            var count = 0;
            foreach (var p in newProviders)
            {
                providers[p.Id] = p;
                if (count == maxCount)
                {
                    break;
                }
            }
            selector = new RandomSelector<string>(providers.Keys);
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


        private abstract class Selector<T>
        {
            protected T[] values;

            public Selector(IEnumerable<T> values)
            {
                if (!values.Any())
                {
                    throw new ArgumentException("Empty values for Selector");
                }
                this.values = values.ToArray();
            }

            public abstract T Select();
        }

        private class RandomSelector<T> : Selector<T>
        {
            public Random rng = new Random();

            public RandomSelector(IEnumerable<T> values) : base(values)
            {
            }

            public override T Select()
            {
                var index = rng.Next(values.Length);
                return values[index];
            }
        }
    }
}
