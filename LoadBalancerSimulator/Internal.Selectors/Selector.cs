using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadBalancerSimulator.Internal.Selectors
{
    abstract class Selector<T>
    {
        protected T[] values;

        public static Selector<T> Create(LoadBalancer.ProviderSelectorType providerSelectorType)
        {
            switch (providerSelectorType)
            {
                case LoadBalancer.ProviderSelectorType.Random: return new RandomSelector<T>();
                case LoadBalancer.ProviderSelectorType.RoundRobin: return new RoundRobinSelector<T>();
            }
            throw new ArgumentException("Invalid providerSelectorType");
        }

        public Selector()
        {
            this.values = Array.Empty<T>();
        }

        public int ValuesCount => values.Length;

        public void UpdateValues(IEnumerable<T> newValues)
        {
            var tmp = newValues.ToArray();
            Array.Sort(tmp);
            values = tmp;
            UpdateState();
        }

        public abstract T Select();

        protected virtual void UpdateState() { }
    }
}