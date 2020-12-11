using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadBalancerSimulator.Selectors
{
    abstract class Selector<T>
    {
        protected T[] values;

        public static Selector<T> Create(LoadBalancer.ProviderSelectorType providerSelectorType, IEnumerable<T> values)
        {
            switch (providerSelectorType)
            {
                case LoadBalancer.ProviderSelectorType.Random: return new RandomSelector<T>(values);
                case LoadBalancer.ProviderSelectorType.RoundRobin: return new RoundRobinSelector<T>(values);
            }
            throw new ArgumentException("Invalid providerSelectorType");
        }

        public Selector(IEnumerable<T> values)
        {
            if (!values.Any())
            {
                throw new ArgumentException("Empty values for Selector");
            }
            this.values = values.ToArray();
            Array.Sort(this.values);
        }

        public abstract T Select();
    }
}