using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// DefaultHttpUtil
    /// </summary>
    internal class DefaultHttpUtil : IHttpUtil
    {
#if NET7_0_OR_GREATER

        private readonly SocketsHttpHandler _httpMessageHandler = new SocketsHttpHandler();

#else

        private readonly HttpClientHandler _httpMessageHandler = new HttpClientHandler();

#endif

        private readonly HttpClient _httpClient;
        private readonly HttpUtilConfig _httpUtilConfig;
        private bool _disposed;

        public PendingCounter PendingCounter { get; } = new PendingCounter();
        public CookieContainer CookieContainer => _httpMessageHandler.CookieContainer;

        public event EventHandler OnDisposed;

        public DefaultHttpUtil(HttpUtilConfig httpUtilConfig)
        {
            PendingCounter.OnStart += (s, a) => LastCallTime = DateTime.Now;
            _httpUtilConfig = httpUtilConfig;
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = httpUtilConfig.BaseAddress,
                Timeout = Timeout.InfiniteTimeSpan,
                MaxResponseContentBufferSize = _httpUtilConfig.MaxResponseContentBufferSize,

#if NET7_0_OR_GREATER

                DefaultRequestVersion = _httpUtilConfig.DefaultRequestVersion,
                DefaultVersionPolicy = _httpUtilConfig.DefaultVersionPolicy,

#endif
            };
            _httpMessageHandler.CookieContainer = _httpUtilConfig.CookieContainer;
            _httpUtilConfig.HttpMessageHandlerAction?.Invoke(_httpMessageHandler);
            _httpUtilConfig.DefaultRequestHeadersAction?.Invoke(_httpClient.DefaultRequestHeaders);
        }

        public DateTime LastCallTime { get; set; }

        public TimeSpan? AutoDisposed => _httpUtilConfig.AutoDisposed;

        public IRequestContext Method(Uri uri, HttpMethod httpMethod)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(IHttpUtil).FullName);
            }

            if (!uri.IsAbsoluteUri && null == _httpClient.BaseAddress)
            {
                throw new ArgumentException($"The URI '{uri}' must be absolute or the HttpUtilConfig BaseAddress must be set.", nameof(uri));
            }

            return new RequestContext(_httpClient, _httpMessageHandler, _httpUtilConfig, uri, httpMethod, PendingCounter);
        }

        public IRequestContext Method(string uri, HttpMethod httpMethod)
        {
            return Method(new Uri(uri, UriKind.RelativeOrAbsolute), httpMethod);
        }

        public IRequestContext Post(Uri uri)
        {
            return Method(uri, HttpMethod.Post);
        }

        public IRequestContext Post(string uri)
        {
            return Method(uri, HttpMethod.Post);
        }

        public IRequestContext Get(Uri uri)
        {
            return Method(uri, HttpMethod.Get);
        }

        public IRequestContext Get(string uri)
        {
            return Method(uri, HttpMethod.Get);
        }

        public IRequestContext PostFormData(Uri uri)
        {
            return Method(uri, new HttpMethod(RequestContext.POST_FORMDATA));
        }

        public IRequestContext PostFormData(string uri)
        {
            return Method(uri, new HttpMethod(RequestContext.POST_FORMDATA));
        }

        public IRequestContext Delete(Uri uri)
        {
            return Method(uri, HttpMethod.Delete);
        }

        public IRequestContext Delete(string uri)
        {
            return Method(uri, HttpMethod.Delete);
        }

        public IRequestContext Put(Uri uri)
        {
            return Method(uri, HttpMethod.Put);
        }

        public IRequestContext Put(string uri)
        {
            return Method(uri, HttpMethod.Put);
        }

        public IRequestContext Head(Uri uri)
        {
            return Method(uri, HttpMethod.Head);
        }

        public IRequestContext Head(string uri)
        {
            return Method(uri, HttpMethod.Head);
        }

        public IRequestContext Options(Uri uri)
        {
            return Method(uri, HttpMethod.Options);
        }

        public IRequestContext Options(string uri)
        {
            return Method(uri, HttpMethod.Options);
        }

        public IRequestContext Trace(Uri uri)
        {
            return Method(uri, HttpMethod.Trace);
        }

        public IRequestContext Trace(string uri)
        {
            return Method(uri, HttpMethod.Trace);
        }

#if NET7_0_OR_GREATER

        public IRequestContext Patch(Uri uri)
        {
            return Method(uri, HttpMethod.Patch);
        }

        public IRequestContext Patch(string uri)
        {
            return Method(uri, HttpMethod.Patch);
        }

        public IRequestContext Connect(Uri uri)
        {
            return Method(uri, HttpMethod.Connect);
        }

        public IRequestContext Connect(string uri)
        {
            return Method(uri, HttpMethod.Connect);
        }

#endif

        public IHttpUtil CancelPendingRequests()
        {
            if (!_disposed)
            {
                _httpClient.CancelPendingRequests();
            }

            return this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                _httpClient.Dispose();
                OnDisposed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}