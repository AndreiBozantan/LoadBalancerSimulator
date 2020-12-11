using System.Threading.Tasks;

using IServiceProvider = LoadBalancerSimulator.IServiceProvider;

namespace LoadBalancerTests
{
    public class SimpleProvider : IServiceProvider
    {
        public SimpleProvider(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public async Task<string> Get()
        {
            await Task.Delay(1000);
            return await Task.FromResult(Id);
        }

        public Task<bool> Check()
        {
            return Task.FromResult(true);
        }
    }
}