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
        private readonly Func<string, IEnumerable<Polly.Policy>> _policyCreatetor;
        //把Policy打包成组合 policy wrpper 进行本地缓存
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWraps;
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        public ResilienceHttpClient(Func<string, IEnumerable<Policy>> policyCreatetor, ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _policyCreatetor = policyCreatetor;
            _policyWraps = new ConcurrentDictionary<string, PolicyWrap>();
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
        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, url, item);
            return await DoPostPutAsync(HttpMethod.Post, url, func, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, Dictionary<string, string> formdata, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, url, formdata);
            return await DoPostPutAsync(HttpMethod.Post, url, func, authorizationToken, requestId, authorizationMethod);
        }

        private HttpRequestMessage CreateHttpRequestMessage<T>(HttpMethod method, string url, T item)
        {
            return new HttpRequestMessage(method, url) { Content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json") };
        }
        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, Dictionary<string, string> formdata)
        {
            return new HttpRequestMessage(method, url) { Content = new FormUrlEncodedContent(formdata) };
        }
        private Task<HttpResponseMessage> DoPostPutAsync(HttpMethod method, string uri, Func<HttpRequestMessage> requestMessageAction, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be either post or put.", nameof(method));
            }
            var origin = GetOriginFromUri(uri);

            return HttpInvoker(origin, async () =>
            {
                var requestMessage = requestMessageAction();
                SetAuthizationHeader(requestMessage);

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
