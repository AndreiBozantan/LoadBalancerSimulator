using System.Threading.Tasks;

using IServiceProvider = LoadBalancerSimulator.IServiceProvider;

namespace LoadBalancerTests
{
    public class SimpleProvider : IServiceProvider
    {
        private readonly int requestDuration;

        public SimpleProvider(string id, int requestDuration = 1000)
        {
            Id = id;
            this.requestDuration = requestDuration;
        }

        public string Id { get; }

        public async Task<string> Get()
        {
            await Task.Delay(this.requestDuration);
            return await Task.FromResult(Id);
        }

        public Task<bool> Check()
        {
            return Task.FromResult(true);
        }
    }
}