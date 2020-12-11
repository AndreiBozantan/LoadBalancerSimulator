using System;
using System.Collections.Generic;

namespace LoadBalancerSimulator.Internal.Selectors
{
     class RandomSelector<T> : Selector<T>
    {
        public Random rng = new Random();

        public RandomSelector(IEnumerable<T> values) : base(values)
        {
        }

        protected override T GetValue()
        {
            var index = rng.Next(values.Length);
            return values[index];
        }
    }
}