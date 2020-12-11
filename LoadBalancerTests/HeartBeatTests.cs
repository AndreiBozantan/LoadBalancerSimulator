using System;
using System.Threading.Tasks;

using Xunit;
using LoadBalancerSimulator;

using IServiceProvider = LoadBalancerSimulator.IServiceProvider;

namespace LoadBalancerTests
{
    public class HeartBeatTests
    {
        public class FailingProvider : IServiceProvider
        {
            public FailingProvider(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public async Task<string> Get()
            {
                return await Task.FromResult(Id);
            }

            public Task<bool> Check()
            {
                return Task.FromResult(false);
            }
        }

        public class RecoveringProvider : IServiceProvider
        {
            private bool status = false;
            public RecoveringProvider(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public async Task<string> Get()
            {
                return await Task.FromResult(Id);
            }

            public Task<bool> Check()
            {
                try
                {
                    return Task.FromResult(status);
                }
                finally
                {
                    status = true;
                }
            }
        }

        [Fact]
        public async Task TestExcludeOnFailure()
        {
            System.Console.WriteLine(nameof(TestExcludeOnFailure));

            var b = new LoadBalancer(10, LoadBalancer.ProviderSelectorType.RoundRobin, TimeSpan.FromSeconds(0.5));
            var providers = new IServiceProvider[] {new SimpleProvider("0"), new FailingProvider("1") };
            b.Register(providers);

            b.DisplayStatus(Console.Out, true);
            Assert.Equal(2, b.ProvidersAliveCount);
            await Task.Delay(1000);
            b.DisplayStatus(Console.Out, true);
            Assert.Equal(1, b.ProvidersAliveCount);

            System.Console.WriteLine("======================");
        }

        [Fact]
        public async Task TestIncludeRecoveredProvider()
        {
            System.Console.WriteLine(nameof(TestIncludeRecoveredProvider));

            var b = new LoadBalancer(10, LoadBalancer.ProviderSelectorType.RoundRobin, TimeSpan.FromSeconds(1));
            var providers = new IServiceProvider[] {new SimpleProvider("1"), new FailingProvider("2"), new RecoveringProvider("3") };
            b.Register(providers);

            b.DisplayStatus(Console.Out, true);
            Assert.Equal(3, b.ProvidersAliveCount);

            await Task.Delay(1100);
            b.DisplayStatus(Console.Out, true);
            Assert.Equal(1, b.ProvidersAliveCount);

            var t = Task.WhenAll(new[] { b.Get(), b.Get(), b.Get() });

            await Task.Delay(500);
            b.DisplayStatus(Console.Out, true);
            Assert.Equal(1, b.ProvidersAliveCount);
            Assert.Equal(2, b.ConcurrentRequestCount);

            await Task.Delay(1000);
            b.DisplayStatus(Console.Out, true);
            Assert.Equal(1, b.ProvidersAliveCount);

            await Task.Delay(1000);
            b.DisplayStatus(Console.Out, true);
            Assert.Equal(2, b.ProvidersAliveCount);

            System.Console.WriteLine("======================");
        }
    }
}
