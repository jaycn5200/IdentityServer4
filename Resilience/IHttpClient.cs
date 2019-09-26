using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Resilience
{
    /// <summary>
    /// IHttpClient
    /// </summary>
    public interface IHttpClient
    {
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
        Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken=null, string requestId = null,
            string authorizationMethod = "Bearer");

        Task<HttpResponseMessage> PostAsync<T>(string url,Dictionary<string,string> formdata, string authorizationToken=null, string requestId = null,
            string authorizationMethod = "Bearer");
    }
}
