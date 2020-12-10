using System.Threading.Tasks;

namespace LoadBalancerSimulator
{
    class Provider
    {
        public Provider(string id)
        {
            Id = id;
        }

        private string Id { get; }

        public async Task<string> Get()
        {
            await Task.Delay(1000);
            return await Task.FromResult(Id);
        }
    }
}