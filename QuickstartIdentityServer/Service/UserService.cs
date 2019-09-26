using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Options;
using QuickstartIdentityServer.Entitys;
using Resilience;

namespace QuickstartIdentityServer.Service
{
    public class UserService : IUserServise
    {
        private IHttpClient _httpClient;
        //private readonly string url = "https://localhost:44393/";

        private string url;

        public UserService( IOptions<ServiceDisvoveryOptions> serviceDisconverOptinos, IDnsQuery dnsQuery, IHttpClient httpClient)
        {
            _httpClient = httpClient;
            var address = dnsQuery.ResolveService("service.consul", serviceDisconverOptinos.Value.UserServiceName);
            var addressList = address.First().AddressList;
            var host = addressList.Any()?addressList.First().ToString():"127.0.0.1";
            var port = address.First().Port;
            url = $"http://{host}:{port}/";
        }

        public async Task<long> CheckOrCreate(string phone)
        {
            var dir = new Dictionary<string, string> { { "phone", phone } };
            var content = new FormUrlEncodedContent(dir);
            var response = await _httpClient.PostAsync(url + "api/User/check", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var userId = await response.Content.ReadAsStringAsync();
                int.TryParse(userId, out int intUserId);
                return Convert.ToInt64(userId);
            }

            return 0;
        }
    }
}
