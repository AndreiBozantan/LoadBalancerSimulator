using System.Threading;
using System;
using System.Threading.Tasks;

namespace LoadBalancerSimulator
{
    class StatefulProvider : IServiceProvider
    {
        public enum ProviderStatus
        {
            Excluded,
            NoHeartBeat,
            Alive,
            Busy
        };

        private int taskCount = 0;

        public int DefaultRequestTimeout = 2000;

        public StatefulProvider(IServiceProvider provider)
        {
            Provider = provider;
        }

        public IServiceProvider Provider { get; }

        public string Id => Provider.Id;

        public bool IsAlive => !Excluded && SuccessfulConsecutiveChecks >= 2;

        public int TaskCount { get => taskCount; private set => taskCount = value; }

        public bool Excluded { get; set; } = false;

        public int SuccessfulConsecutiveChecks { get; private set; } = 2;

        public ProviderStatus Status
        {
            get
            {
                if (Excluded)
                    return ProviderStatus.Excluded;
                if (SuccessfulConsecutiveChecks < 2)
                    return ProviderStatus.NoHeartBeat;
                if (TaskCount == 0)
                    return ProviderStatus.Alive;
                return ProviderStatus.Busy;
            }
        }

        public string StatusString
        {
            get
            {
                switch (Status)
                {
                    case ProviderStatus.Excluded: return "EXCL";
                    case ProviderStatus.NoHeartBeat: return "DEAD";
                    case ProviderStatus.Alive: return "LIVE";
                    case ProviderStatus.Busy: return "BUSY";
                }
                return "?";
            }
        }

        public async Task<string> Get()
        {
            using (var cts = new CancellationTokenSource(DefaultRequestTimeout))
            {
                try
                {
                    Interlocked.Increment(ref taskCount);
                    return await Task.Run(() => Provider.Get(), cts.Token);
                }
                finally
                {
                    Interlocked.Decrement(ref taskCount);
                }
            }
        }

        public async Task<bool> Check()
        {
            if (Excluded)
            {
                return false;
            }
            try
            {
                using (var cts = new CancellationTokenSource(DefaultRequestTimeout))
                {
                    var ret = await Task.Run(() => Provider.Check(), cts.Token);
                    if (!ret)
                    {
                        SuccessfulConsecutiveChecks = 0;
                        return false;
                    }
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