using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadBalancerSimulator.Internal.Selectors
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
            this.values = GetNewValues(values);
        }

        public int ValuesCount => values.Length;

        public void UpdateValues(IEnumerable<T> newValues)
        {
            lock (this)
            {
                values = GetNewValues(newValues);
                UpdateState();
            }
        }

        public T Select()
        {
            lock (this)
            {
                return GetValue();
            }
        }

        protected abstract T GetValue();

        protected virtual void UpdateState()
        {
        }

        private static T[] GetNewValues(IEnumerable<T> values)
        {
            var ret = values.ToArray();
            Array.Sort(ret);
            return ret;
        }
    }
}