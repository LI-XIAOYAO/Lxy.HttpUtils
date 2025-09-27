using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// HttpUtilConfig should be configured before calling or after disposed.
    /// </summary>
    public sealed class HttpUtilConfig
    {
        private TimeSpan? _autoDisposed = TimeSpan.FromMinutes(30);
        private TimeSpan _timeout = TimeSpan.FromSeconds(100);
        private int _maxResponseContentBufferSize = int.MaxValue;
        private CookieContainer _cookieContainer = new CookieContainer();
        private Uri _baseAddress;
        private string _userAgent;
        private string _authorization;
        private string _contentType = "application/json";

#if NET7_0_OR_GREATER

        private Version _defaultRequestVersion = HttpVersion.Version20;

#else

        private Version _defaultRequestVersion = HttpVersion.Version11;

#endif

        /// <summary>
        ///  Gets or sets the HTTP message version.
        /// </summary>
        public Version DefaultRequestVersion
        {
            get => _defaultRequestVersion;
            set
            {
                _defaultRequestVersion = value ?? throw new ArgumentNullException(nameof(DefaultRequestVersion));
            }
        }

#if NET7_0_OR_GREATER

        /// <summary>
        /// <inheritdoc cref="HttpClient.DefaultVersionPolicy"/>
        /// </summary>
        public HttpVersionPolicy DefaultVersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;

#endif

        /// <summary>
        /// HttpUtilConfig should be configured before calling or after disposed.
        /// </summary>
        public HttpUtilConfig()
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
        }

        internal MediaTypeHeaderValue ContentTypeHeaderValue { get; set; } = MediaTypeHeaderValue.Parse("application/json");

        /// <summary>
        /// Gets or sets the default managed cookie container object.
        /// </summary>
        public CookieContainer CookieContainer { get => _cookieContainer; set => _cookieContainer = value ?? throw new ArgumentNullException(nameof(CookieContainer)); }

        /// <summary>
        /// Auto dispose, minimum value of 3 minutes, default value of 30 minutes.
        /// </summary>
        public TimeSpan? AutoDisposed
        {
            get => _autoDisposed;
            set
            {
                if (null == value)
                {
                    _autoDisposed = null;

                    return;
                }

                var timeSpan = TimeSpan.FromMinutes(3);
                _autoDisposed = value > timeSpan ? value : timeSpan;
            }
        }

        /// <summary>
        /// <inheritdoc cref="HttpClient.Timeout"/> default 100s.
        /// </summary>
        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                if (value != System.Threading.Timeout.InfiniteTimeSpan && (value <= TimeSpan.Zero || value > TimeSpan.FromMilliseconds(int.MaxValue)))
                {
                    throw new ArgumentOutOfRangeException(nameof(Timeout));
                }

                _timeout = value;
            }
        }

        /// <summary>
        /// RetryCount
        /// </summary>
        internal int RetryCount { get; set; }

        /// <summary>
        /// RetryOptions
        /// </summary>
        internal RetryOptions RetryOptions { get; set; } = RetryOptions.Timeout;

        /// <summary>
        /// RetryPolicyFunc
        /// </summary>
        internal Func<int, int> RetryPolicyFunc { get; set; }

        /// <summary>
        /// <inheritdoc cref="HttpClient.MaxResponseContentBufferSize"/>
        /// </summary>
        public int MaxResponseContentBufferSize
        {
            get => _maxResponseContentBufferSize;
            set
            {
                if (value <= 0 || value > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxResponseContentBufferSize));
                }

                _maxResponseContentBufferSize = value;
            }
        }

        /// <summary>
        /// <inheritdoc cref="HttpClient.BaseAddress"/>
        /// </summary>
        public Uri BaseAddress
        {
            get => _baseAddress;
            set
            {
                if (null != _baseAddress)
                {
                    throw new ArgumentException("BaseAddress can only be set once.", nameof(BaseAddress));
                }

                if (null == value)
                {
                    throw new ArgumentNullException(nameof(BaseAddress));
                }

                if (!value.IsAbsoluteUri)
                {
                    throw new ArgumentException("The URI must be absolute.", nameof(BaseAddress));
                }

                _baseAddress = value;
            }
        }

        /// <summary>
        /// <inheritdoc cref="HttpResponseMessage.EnsureSuccessStatusCode()"/>
        /// </summary>
        public bool EnsureSuccessStatusCode { get; set; } = false;

        /// <summary>
        /// The encoding to use for the content.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// The serializer settings to use for <see cref="HttpRequestMessage.Content"/>.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// DefaultRequestHeadersAction
        /// </summary>
        internal Action<HttpRequestHeaders> DefaultRequestHeadersAction { get; private set; }

        /// <summary>
        /// Set the value of the Content-Type header for default HTTP request.
        /// </summary>
        public string ContentType
        {
            get => _contentType;
            set
            {
                ContentTypeHeaderValue = null == value ? null : MediaTypeHeaderValue.Parse(value);
                _contentType = value;
            }
        }

        /// <summary>
        /// Set the value of the User-Agent header for default HTTP request.
        /// </summary>
        public string UserAgent
        {
            get => _userAgent;
            set
            {
                _userAgent = value;

                SetDefaultRequestHeaders(c =>
                {
                    c.UserAgent.Clear();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        c.TryAddWithoutValidation("User-Agent", value);
                    }
                });
            }
        }

        /// <summary>
        /// Set the value of the Authorization header for default HTTP request.
        /// </summary>
        public string Authorization
        {
            get => _authorization;
            set
            {
                AuthenticationHeaderValue authenticationHeaderValue = null;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    authenticationHeaderValue = AuthenticationHeaderValue.Parse(value);
                }

                _authorization = value;

                SetDefaultRequestHeaders(c => c.Authorization = authenticationHeaderValue);
            }
        }

#if NET7_0_OR_GREATER

        /// <summary>
        /// Set <see cref="HttpMessageHandler"/>
        /// </summary>
        internal Action<SocketsHttpHandler> HttpMessageHandlerAction { get; private set; }

        /// <summary>
        /// Set <see cref="SocketsHttpHandler"/>
        /// </summary>
        /// <param name="action"></param>
        public HttpUtilConfig SetHttpMessageHandler(Action<SocketsHttpHandler> action)
        {
            HttpMessageHandlerAction += action;

            return this;
        }

#else

        /// <summary>
        /// HttpClientHandlerAction
        /// </summary>
        internal Action<HttpClientHandler> HttpMessageHandlerAction { get; private set; }

        /// <summary>
        /// Set <see cref="HttpClientHandler"/>
        /// </summary>
        /// <param name="action"></param>
        public HttpUtilConfig SetHttpMessageHandler(Action<HttpClientHandler> action)
        {
            HttpMessageHandlerAction += action;

            return this;
        }

#endif

        /// <summary>
        /// Set header for default HTTP request.
        /// </summary>
        /// <param name="action"></param>
        public HttpUtilConfig SetDefaultRequestHeaders(Action<HttpRequestHeaders> action)
        {
            DefaultRequestHeadersAction += action;

            return this;
        }

        /// <summary>
        /// Retry when failed, default <see cref="RetryOptions.Timeout" />.
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="policy"><![CDATA[Func<Retry, MillisecondsDelay>]]></param>
        /// <param name="retryOptions"></param>
        public HttpUtilConfig SetRetry(int retryCount, Func<int, int> policy, RetryOptions retryOptions = RetryOptions.Timeout)
        {
            return SetRetry(retryCount, retryOptions, policy);
        }

        /// <summary>
        /// Retry when failed, default <see cref="RetryOptions.Timeout" />.
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="retryOptions"></param>
        /// <param name="policy"><![CDATA[Func<Retry, MillisecondsDelay>]]></param>
        public HttpUtilConfig SetRetry(int retryCount, RetryOptions retryOptions = RetryOptions.Timeout, Func<int, int> policy = null)
        {
            RetryCount = retryCount;
            RetryOptions = retryOptions;
            RetryPolicyFunc = policy;

            return this;
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpUtilConfig AddCookie(Uri uri, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            CookieContainer.Add(new Cookie(name, value, "/", uri.Host));

            return this;
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpUtilConfig AddCookie(string name, string value)
        {
            if (null == BaseAddress)
            {
                throw new ArgumentNullException(nameof(BaseAddress));
            }

            return AddCookie(BaseAddress, name, value);
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpUtilConfig AddCookies(IDictionary<string, string> cookies)
        {
            if (cookies is null)
            {
                throw new ArgumentNullException(nameof(cookies));
            }

            foreach (var cookie in cookies)
            {
                AddCookie(cookie.Key, cookie.Value);
            }

            return this;
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpUtilConfig AddCookies(Uri uri, IDictionary<string, string> cookies)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (cookies is null)
            {
                throw new ArgumentNullException(nameof(cookies));
            }

            foreach (var cookie in cookies)
            {
                AddCookie(uri, cookie.Key, cookie.Value);
            }

            return this;
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cookieHeaders"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public HttpUtilConfig AddCookies(Uri uri, string cookieHeaders)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (string.IsNullOrWhiteSpace(cookieHeaders))
            {
                throw new ArgumentException(nameof(cookieHeaders));
            }

            CookieContainer.SetCookies(uri, cookieHeaders.Replace(";", ","));

            return this;
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookieHeaders"></param>
        /// <returns></returns>
        public HttpUtilConfig AddCookies(string cookieHeaders)
        {
            return AddCookies(BaseAddress, cookieHeaders);
        }

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpUtilConfig AddCookies(CookieCollection cookies)
        {
            if (cookies is null)
            {
                throw new ArgumentNullException(nameof(cookies));
            }

            CookieContainer.Add(cookies);

            return this;
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public HttpUtilConfig Clone()
        {
            var httpUtilConfig = new HttpUtilConfig
            {
                AutoDisposed = AutoDisposed,
                Timeout = Timeout,
                RetryCount = RetryCount,
                RetryPolicyFunc = null != RetryPolicyFunc ? (Func<int, int>)RetryPolicyFunc.Clone() : null,
                Authorization = Authorization,
                ContentType = ContentType,
                ContentTypeHeaderValue = MediaTypeHeaderValue.Parse(_contentType),
                CookieContainer = CookieContainer,
                DefaultRequestHeadersAction = null != DefaultRequestHeadersAction ? (Action<HttpRequestHeaders>)DefaultRequestHeadersAction.Clone() : null,
                Encoding = Encoding,
                EnsureSuccessStatusCode = EnsureSuccessStatusCode,

#if NET7_0_OR_GREATER

                HttpMessageHandlerAction = null != HttpMessageHandlerAction ? (Action<SocketsHttpHandler>)HttpMessageHandlerAction.Clone() : null,
                DefaultVersionPolicy = DefaultVersionPolicy,

#else

                HttpMessageHandlerAction = null != HttpMessageHandlerAction ? (Action<HttpClientHandler>)HttpMessageHandlerAction.Clone() : null,

#endif

                JsonSerializerSettings = null != JsonSerializerSettings ? new JsonSerializerSettings(JsonSerializerSettings) : null,
                MaxResponseContentBufferSize = MaxResponseContentBufferSize,
                UserAgent = UserAgent,
                DefaultRequestVersion = DefaultRequestVersion,
                RetryOptions = RetryOptions
            };

            if (null != BaseAddress)
            {
                httpUtilConfig.BaseAddress = BaseAddress;
            }

            return httpUtilConfig;
        }
    }
}