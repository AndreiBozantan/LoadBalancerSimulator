using System.Reflection.Emit;
using System;

using LoadBalancerTests;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var t0 = new BasicTests();
            t0.GetRandomInvocationSuccess().Wait();

            var t1 = new ClusterCapacityTests();
            t1.TestClusterCapacityLimit1().Wait();
            t1.TestClusterCapacityLimit2().Wait();

            var t2 = new HeartBeatTests();
            t2.TestExcludeOnFailure().Wait();
            t2.TestIncludeRecoveredProvider().Wait();
        }
    }
}
