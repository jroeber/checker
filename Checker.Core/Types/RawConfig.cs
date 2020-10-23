using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Checker.Types
{
    public class RawConfig
    {
        public List<RawHostConfig> Hosts { get; set; } = new List<RawHostConfig>();
        public List<RawDnsConfig> Dns { get; set; } = new List<RawDnsConfig>();

        public string Metrics { get; set; } = "";

        public static RawConfig Create(string filepath)
        {
            var configStr = File.ReadAllText(filepath);
            var deserializerInput = new StringReader(configStr);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<RawConfig>(deserializerInput);
        }
    }

    public class RawHostConfig
    {
        public string Host { get; set; } = ""; // Technically this is a "socket address"
        public string Type { get; set; } = "";
        public int Rate { get; set; } = 15;
        public int Timeout { get; set; } = 15;

        public int ExpectedCode { get; set; } = 200; // Only used by HttpConnector
    }

    public class RawDnsConfig
    {
        public string Query { get; set; } = "";
        public string Type { get; set; } = "";
        public string Expect { get; set; } = "";
        public string Resolver { get; set; } = "";
    }
}