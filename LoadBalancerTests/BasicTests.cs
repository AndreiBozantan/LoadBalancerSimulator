using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Xunit;
using LoadBalancerSimulator;

namespace LoadBalancerTests
{
    public class BasicTests
    {
        [Theory]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        public void TestRegisterCapacity(int capacity)
        {
            var lb = new LoadBalancer(capacity, LoadBalancer.ProviderSelectorType.Random, TimeSpan.FromSeconds(2));

            var providers1 = Enumerable.Range(0, 6).Select(i => new SimpleProvider(i.ToString()));
            var c1 = lb.Register(providers1);
            Assert.Equal(6, lb.TotalProvidersCount);
            Assert.Equal(6, c1);

            var providers2 = Enumerable.Range(6, 10).Select(i => new SimpleProvider(i.ToString()));
            var c2 = lb.Register(providers2);
            Assert.Equal(capacity, lb.TotalProvidersCount);
            Assert.Equal(capacity - 6, c2);
        }

        [Fact]
        public void TestDuplicateProviderId()
        {
            var lb = new LoadBalancer(10, LoadBalancer.ProviderSelectorType.Random, TimeSpan.FromSeconds(2));

            var providers1 = Enumerable.Range(0, 5).Select(i => new SimpleProvider(i.ToString()));
            var c1 = lb.Register(providers1);
            Assert.Equal(5, lb.TotalProvidersCount);
            Assert.Equal(5, c1);

            lb.Register(new[] { new SimpleProvider("0") });
            Assert.Equal(5, lb.TotalProvidersCount);

            lb.Register(new[] { new SimpleProvider("6") });
            Assert.Equal(6, lb.TotalProvidersCount);
        }

        [Fact]
        public void GetWithNoProvidersThrows()
        {
            var lb = new LoadBalancer(10, LoadBalancer.ProviderSelectorType.Random, TimeSpan.FromSeconds(2));
            Assert.ThrowsAsync<InvalidOperationException>(() => lb.Get());
        }

        [Fact]
        public async Task GetRandomInvocationSuccess()
        {
            System.Console.WriteLine("Random invocation test");
            var b = new LoadBalancer(5, LoadBalancer.ProviderSelectorType.Random, TimeSpan.FromSeconds(0.8));
            var ids = new HashSet<string>{"0", "1", "2", "3", "4"};
            b.Register(ids.Select(id => new SimpleProvider(id, 2000)));

            var t = Task.WhenAll(Enumerable.Range(0, 10).Select(async i =>
            {
                try
                {
                    System.Console.WriteLine($"Start request {i}");
                    var r = await b.Get();
                    Assert.Contains(r, ids);
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
                await Task.Delay(500);
                b.DisplayStatus(Console.Out, true);
            }

            System.Console.WriteLine("======================");
        }

        [Fact]
        public async void GetRoundRobinInvocationSuccess()
        {
            var lb = new LoadBalancer(5, LoadBalancer.ProviderSelectorType.RoundRobin, TimeSpan.FromSeconds(2));
            lb.MaxParallelRequestsPerProvider = 5;
            lb.Register(Enumerable.Range(0, 5).Select(id => new SimpleProvider(id.ToString())));
            var results = await Task.WhenAll(Enumerable.Range(0, 15).Select(_ => lb.Get()));
            Assert.Equal(new[] { "0", "1", "2", "3", "4", "0", "1", "2", "3", "4", "0", "1", "2", "3", "4"}, results);
        }
    }
}
