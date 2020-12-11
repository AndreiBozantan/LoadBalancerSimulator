using System.Threading.Tasks;

namespace LoadBalancerSimulator
{
    public interface IServiceProvider
    {
        string Id { get; }

        Task<string> Get();

        Task<bool> Check();
    }
}