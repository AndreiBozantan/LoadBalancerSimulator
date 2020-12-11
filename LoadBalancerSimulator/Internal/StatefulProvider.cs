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

        public string Id => Provider.Id;

        public bool InService => !Excluded && SuccessfulConsecutiveChecks >= 2;

        public bool Excluded { get; set; }

        public int SuccessfulConsecutiveChecks { get; private set; } = 0;

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
                    SuccessfulConsecutiveChecks = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Heartbeat check failed for provider with Id {Id}");
                Console.WriteLine(ex);
                SuccessfulConsecutiveChecks = 0;
                return false;
            }
            SuccessfulConsecutiveChecks++;
            return true;
        }
    }
}