using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Xunit;
using LoadBalancerSimulator;

namespace LoadBalancerTests
{
    public class ClusterCapacityTests
    {

        [Fact]
        public async Task TestClusterCapacityLimit1()
        {
            System.Console.WriteLine(nameof(TestClusterCapacityLimit1));

            var b = new LoadBalancer(10, LoadBalancer.ProviderSelectorType.RoundRobin, TimeSpan.FromSeconds(2));
            var providers = new[] {new SimpleProvider("0"), new SimpleProvider("1") };
            b.Register(providers);

            Task t1 = b.Get();
            Assert.Equal(1, b.ConcurrentRequestCount);
            b.DisplayStatus(Console.Out, true);

            Task t2 = b.Get();
            Assert.Equal(2, b.ConcurrentRequestCount);
            b.DisplayStatus(Console.Out, true);

            Task t3 = b.Get();
            Assert.Equal(3, b.ConcurrentRequestCount);
            b.DisplayStatus(Console.Out, true);

            Task t4 = b.Get();
            Assert.Equal(4, b.ConcurrentRequestCount);
            b.DisplayStatus(Console.Out, true);

            await Assert.ThrowsAsync<Exception>(async () => await b.Get());
            System.Console.WriteLine("***Request failed: cluster parallel requests capacity limit reached!");
            b.DisplayStatus(Console.Out, true);
            Assert.Equal(4, b.ConcurrentRequestCount);

            System.Console.WriteLine("======================");
        }

        [Fact]
        public async Task TestClusterCapacityLimit2()
        {
            System.Console.WriteLine(nameof(TestClusterCapacityLimit2));

            var b = new LoadBalancer(10, LoadBalancer.ProviderSelectorType.RoundRobin, TimeSpan.FromSeconds(2));
            b.Register(Enumerable.Range(0, 2).Select(id => new SimpleProvider(id.ToString(), 4000)));
            var t = Task.WhenAll(Enumerable.Range(1, 6).Select(async i =>
            {
                try
                {
                    System.Console.WriteLine($"Start request {i}");
                    if (i < 5)
                    {
                        Assert.Equal(i - 1, b.ConcurrentRequestCount);
                    }
                    else
                    {
                        Assert.Equal(4, b.ConcurrentRequestCount);
                    }
                    await b.Get();
                    if (i >= 5)
                    {
                        Assert.True(false, "capacity exception not raised");
                    }
                    System.Console.WriteLine($"End request {i};");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Request {i} failed.");
                    System.Console.WriteLine(ex);
                }
            }));

            while (!t.IsCompleted)
            {
                await Task.Delay(1000);
                b.DisplayStatus(Console.Out, true);
            }

            System.Console.WriteLine("======================");
        }
    }
}
