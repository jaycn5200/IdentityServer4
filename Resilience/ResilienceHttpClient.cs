using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Resilience
{
    public class ResilienceHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;
        //根据URL 原始地址去创建policy
        private readonly Func<string, IEnumerable<Policy>> _policyCreatetor;
        //把Policy打包成组合 policy wrpper 进行本地缓存
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWraps;
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        public ResilienceHttpClient(HttpClient httpClient, Func<string, IEnumerable<Policy>> policyCreatetor, ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor, ConcurrentDictionary<string, PolicyWrap> policyWraps)
        {
            _httpClient = httpClient;
            _policyCreatetor = policyCreatetor;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _policyWraps = policyWraps;
        }
        /// <summary>
        /// PostAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="item"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="requestId"></param>
        /// <param name="authorizationMethod"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync(HttpMethod.Post, url, item, authorizationToken, requestId, authorizationMethod);
        }

        private Task<HttpResponseMessage> DoPostPutAsync<T>(HttpMethod method, string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be either post or put.", nameof(method));
            }

            var origin = GetOriginFromUri(uri);

            return HttpInvoker(origin, async () =>
            {
                var requestMessage = new HttpRequestMessage(method, uri);

                SetAuthizationHeader(requestMessage);

                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
        {
            var normalizeOrirgin = NormalizeOrigin(origin);
            if (!_policyWraps.TryGetValue(normalizeOrirgin, out PolicyWrap policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreatetor(normalizeOrirgin).ToArray());
                _policyWraps.TryAdd(normalizeOrirgin, policyWrap);

            }
            return await policyWrap.ExecuteAsync(action, new Context(normalizeOrirgin));
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }

        private void SetAuthizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }
        }
    }
}
