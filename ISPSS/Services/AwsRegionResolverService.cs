using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace ISPSS.Services
{
    public class AwsRegionResolverService
    {
        private readonly List<AwsIpRange> awsIpRanges;

        public AwsRegionResolverService(string awsIpRangesJsonPath)
        {
            string json = File.ReadAllText(awsIpRangesJsonPath);
            AwsIpRanges awsIpRangesData = JsonConvert.DeserializeObject<AwsIpRanges>(json);
            awsIpRanges = awsIpRangesData.Prefixes.ToList();
        }

        public string GetAwsRegionByIpAddress(string ipAddress)
        {
            var matchingRange = awsIpRanges.FirstOrDefault(range => IsIpAddressInRange(ipAddress, range.ip_prefix));

            return matchingRange?.region;
        }

        private bool IsIpAddressInRange(string ipAddress, string cidr)
        {
            var ipRange = IPNetwork.Parse(cidr);
            var ip = IPAddress.Parse(ipAddress);

            return ipRange.Contains(ip);
        }
    }

    public class AwsIpRange
    {
        public string ip_prefix { get; set; }
        public string region { get; set; }
    }

    public class AwsIpRanges
    {
        public IEnumerable<AwsIpRange> Prefixes { get; set; }
    }
}
