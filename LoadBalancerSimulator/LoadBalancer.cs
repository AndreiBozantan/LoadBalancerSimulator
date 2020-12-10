using System.Linq;
using System.Collections.Generic;

namespace LoadBalancerSimulator
{
    public class LoadBalancer
    {
        private Dictionary<string, Provider> providers = new Dictionary<string, Provider>();

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
            return count;
        }
    }
}
