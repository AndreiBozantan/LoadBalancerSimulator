using System.Collections.Generic;

namespace LoadBalancerSimulator.Internal.Selectors
{
    class RoundRobinSelector<T> : Selector<T>
    {
        public int index = 0;

        public RoundRobinSelector(IEnumerable<T> values) : base(values)
        {
        }

        public override T Select()
        {
            lock (this)
            {
                var val = values[index];
                index = (index + 1) % values.Length;
                return val;
            }
        }
    }
}