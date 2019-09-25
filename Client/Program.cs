using System;
using System.Net;
using DnsClient;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var dnsQuery = new LookupClient(IPAddress.Parse("127.0.0.1"),8600);
            var result = dnsQuery.ResolveService("service.consul", "servicename");
            Console.WriteLine("Hello World!");
        }
    }
}
