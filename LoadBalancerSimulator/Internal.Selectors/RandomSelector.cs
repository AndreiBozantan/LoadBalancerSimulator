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

        public override T Select()
        {
            var index = rng.Next(values.Length);
            return values[index];
        }
    }
}