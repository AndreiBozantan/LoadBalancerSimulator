using System.Threading.Tasks;

namespace LoadBalancerSimulator
{
    class StatefulProvider : IServiceProvider
    {
        public StatefulProvider(IServiceProvider provider)
        {
            Provider = provider;
            Excluded = false;
        }

        public IServiceProvider Provider { get; }

        public bool Excluded { get; set; }


        public string Id => Provider.Id;

        public Task<string> Get()
        {
            return Provider.Get();
        }
    }
}