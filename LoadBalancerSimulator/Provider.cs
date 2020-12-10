using System.Threading.Tasks;

namespace LoadBalancerSimulator
{
    public class Provider
    {
        public Provider(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public async Task<string> Get()
        {
            await Task.Delay(1000);
            return await Task.FromResult(Id);
        }
    }
}