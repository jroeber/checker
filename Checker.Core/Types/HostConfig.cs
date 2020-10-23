namespace Checker.Types
{
    public abstract class HostConfig
    {
        public string Host { get; }
        public int Rate { get; }
        public int Timeout { get; }

        public HostConfig(string host, int rate = 10, int timeout = 10)
        {
            Host = host;
            Rate = rate;
            Timeout = timeout;
        }
    }
}