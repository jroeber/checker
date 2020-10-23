namespace Checker.Types
{
    public class PingConfig : HostConfig
    {
        public PingConfig(string host, int rate = 10, int timeout = 10) : base(host, rate, timeout)
        {
        }
    }
}