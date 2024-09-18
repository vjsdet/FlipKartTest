using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FrameworkSupport
{
    public class HttpClientHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool IgnoreSSLErrors { get; set; } = true;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        public string BearerToken { get; set; } = null;

        public Uri Url { get; set; } = null;

        public HttpClientHelper(Uri url)
        {
            Url = url;
        }

        public HttpClientHelper(string url) : this(new Uri(url))
        {
        }

        private HttpClientHandler _handler = null;

        private Dictionary<string, string> CookieDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Dispose of any existing HttpClientHandler objects we may have a handle to
        /// </summary>
        private void DisposeHandler()
        {
            _handler?.Dispose();
        }

        /// <summary>
        /// Build and store a handler suitable for trusted servers and dev - does not throw exception if no/bad SSL cert
        /// </summary>
        /// <returns></returns>
        private HttpClientHandler IgnoreCertErrorsHandler()
        {
            DisposeHandler();

            _handler = new HttpClientHandler();
            _handler.ServerCertificateCustomValidationCallback = (m, c, ch, p) => true;
            return _handler;
        }

        /// <summary>
        /// Build and stor a handler that contains the list of cookies to use
        /// </summary>
        /// <returns></returns>
        private HttpClientHandler ClientWithCookiesHandler()
        {
            DisposeHandler();

            _handler = new HttpClientHandler();
            CookieContainer cookieContainer = new CookieContainer();

            if (CookieDictionary != null && CookieDictionary.Keys.Count > 0)
            {
                foreach (var key in CookieDictionary.Keys)
                {
                    cookieContainer.Add(new Cookie(key, CookieDictionary[key]));
                }
            }

            _handler.CookieContainer = cookieContainer;
            return _handler;
        }

        /// <summary>
        /// Adds the cookie values to the dictionary so they will be used on the client call
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <param name="value">The value for the cookie</param>
        public void AddCookie(string name, string value)
        {
            CookieDictionary.SafeAdd<string, string>(name, value);
        }

        /// <summary>
        /// Build a HttpClient with required settings
        /// </summary>
        /// <returns>The client</returns>
        private HttpClient BuildClient()
        {
            HttpClient client = null;
            if (IgnoreSSLErrors)
            {
                client = new HttpClient(IgnoreCertErrorsHandler(), true);
            }
            else
            {
                client = CookieDictionary != null && CookieDictionary.Keys.Count > 0 ? new HttpClient(ClientWithCookiesHandler()) : new HttpClient();
            }

            client.Timeout = Timeout;

            if (!string.IsNullOrEmpty(BearerToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
            }

            return client;
        }

        /// <summary>
        /// Perform a GET and return the response
        /// </summary>
        /// <returns>The response</returns>
        public async Task<HttpResponseMessage> DoGet()
        {
            using (HttpClient client = BuildClient())
            {
                return await client.GetAsync(Url);
            }
        }

        /// <summary>
        /// Perform a POST with the passed string reprenstation of JSON and return the result
        /// </summary>
        /// <param name="json">A stringified JSON object</param>
        /// <returns>The response</returns>
        public async Task<HttpResponseMessage> DoJsonPost(string json)
        {
            using (HttpClient client = BuildClient())
            {
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return await client.PostAsync(Url, content);
            }
        }

        /// <summary>
        /// When we leave, make sure to dispose of any existing HttpClientHandlers
        /// </summary>
        ~HttpClientHelper()
        {
            DisposeHandler();
        }
    }
}