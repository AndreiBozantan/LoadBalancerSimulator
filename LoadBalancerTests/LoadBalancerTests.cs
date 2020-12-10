using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Xunit;
using LoadBalancerSimulator;

namespace LoadBalancerTests
{
    public class UnitTest1
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
            var lb = new LoadBalancer(capacity);

            var providers1 = Enumerable.Range(0, 6).Select(i => new Provider(i.ToString()));
            var c1 = lb.Register(providers1);
            Assert.Equal(6, lb.ProvidersCount);
            Assert.Equal(6, c1);

            var providers2 = Enumerable.Range(6, 6).Select(i => new Provider(i.ToString()));
            var c2 = lb.Register(providers1);
            Assert.Equal(capacity, lb.ProvidersCount);
            Assert.Equal(capacity - 6, c2);
        }

        [Fact]
        public void TestDuplicateProviderId()
        {
            var lb = new LoadBalancer(10);

            var providers1 = Enumerable.Range(0, 5).Select(i => new Provider(i.ToString()));
            var c1 = lb.Register(providers1);
            Assert.Equal(5, lb.ProvidersCount);
            Assert.Equal(5, c1);

            lb.Register(new[] { new Provider("0") });
            Assert.Equal(5, lb.ProvidersCount);

            lb.Register(new[] { new Provider("6") });
            Assert.Equal(6, lb.ProvidersCount);
        }

        [Fact]
        public void GetWithNoProvidersThrows()
        {
            var lb = new LoadBalancer(10);
            Assert.ThrowsAsync<InvalidOperationException>(() => lb.Get());
        }

        [Fact]
        public async void GetRandomInvocationSuccess()
        {
            var lb = new LoadBalancer(5);
            var ids = new HashSet<string>{"0", "1", "2", "3", "4"};
            lb.Register(ids.Select(id => new Provider(id)));
            var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => lb.Get()));
            foreach (var r in results)
            {
                Assert.Contains(r, ids);
            }
        }
    }
}
