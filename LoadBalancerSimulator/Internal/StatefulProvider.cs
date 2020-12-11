using System;
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

        public async Task<bool> Check()
        {
            if (Excluded)
            {
                return false;
            }
            try
            {
                var ret = await Provider.Check();
                if (!ret)
                {
                    Excluded = true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Heartbeat check failed for provider with Id {Id}");
                Console.WriteLine(ex);
                Excluded = true;
                return false;
            }
            return true;
        }
    }
}