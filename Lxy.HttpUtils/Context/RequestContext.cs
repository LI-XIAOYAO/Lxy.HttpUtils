using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// RequestContext
    /// </summary>
    internal class RequestContext : IRequestContext
    {
        internal const string POST_FORMDATA = nameof(POST_FORMDATA);

#if NET7_0_OR_GREATER

        private readonly SocketsHttpHandler _httpMessageHandler;

#else

        private readonly HttpClientHandler _httpMessageHandler;

#endif

        private readonly HttpClient _httpClient;
        private readonly HttpUtilConfig _httpUtilConfig;
        private Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly PendingCounter _pendingCounter;
        private readonly bool _isFormData;
        private bool _disposed;
        private TimeSpan _timeout;
        private Encoding _encoding;
        private bool _ensureSuccessStatusCode;
        private HttpCompletionOption _httpCompletionOption = HttpCompletionOption.ResponseContentRead;
        private Action<HttpRequestMessage> _httpRequestMessageAction;
        private Action<HttpContent> _httpContentAction;
        private HttpContent _httpContent;
        private Version _version;
        private JsonSerializerSettings _jsonSerializerSettings;
        private int _retryCount;
        private Func<int, int> _retryPolicy;
        private RetryOptions _retryOptions;

        private Uri Uri => _uri.IsAbsoluteUri ? _uri : new Uri(_httpClient.BaseAddress, _uri);

        public CookieContainer CookieContainer => _httpMessageHandler.CookieContainer;

        public RequestContext(HttpClient httpClient, HttpMessageHandler httpMessageHandler, HttpUtilConfig httpUtilConfig, Uri uri, HttpMethod httpMethod, PendingCounter pendingCounter)
        {
            _httpClient = httpClient;
            _uri = uri;
            _httpMethod = httpMethod;
            _pendingCounter = pendingCounter;

#if NET7_0_OR_GREATER

            _httpMessageHandler = (SocketsHttpHandler)httpMessageHandler;

#else

            _httpMessageHandler = (HttpClientHandler)httpMessageHandler;

#endif

            _httpUtilConfig = httpUtilConfig;
            _timeout = httpUtilConfig.Timeout;
            _retryCount = httpUtilConfig.RetryCount;
            _retryPolicy = httpUtilConfig.RetryPolicyFunc;
            _retryOptions = httpUtilConfig.RetryOptions;
            _encoding = httpUtilConfig.Encoding;
            _ensureSuccessStatusCode = httpUtilConfig.EnsureSuccessStatusCode;
            _version = httpUtilConfig.DefaultRequestVersion;

            if (null != _httpUtilConfig.JsonSerializerSettings)
            {
                _jsonSerializerSettings = new JsonSerializerSettings(_httpUtilConfig.JsonSerializerSettings);
            }

            if (POST_FORMDATA == httpMethod.Method)
            {
                _isFormData = true;
                _httpMethod = HttpMethod.Post;
            }
        }

        public IRequestContext AddHeader(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            _httpRequestMessageAction += c => c.Headers.TryAddWithoutValidation(name, value);

            return this;
        }

        public IRequestContext AddHeaders(IDictionary<string, string> headers)
        {
            if (headers is null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            _httpRequestMessageAction += c =>
            {
                foreach (var header in headers)
                {
                    c.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            };

            return this;
        }

        public IRequestContext AddQuery(string name, object value, bool isEscape = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            AddQuery(new Dictionary<string, object> { { name, value } }, isEscape);

            return this;
        }

        public IRequestContext AddQuery(IDictionary<string, object> querys, bool isEscape = true)
        {
            if (querys is null)
            {
                throw new ArgumentNullException(nameof(querys));
            }

            AddQuery(string.Join("&", querys.Select(c =>
            {
                if (string.IsNullOrWhiteSpace(c.Key))
                {
                    throw new ArgumentNullException(nameof(querys));
                }

                return $"{c.Key}={(null != c.Value && isEscape ? Uri.EscapeDataString(c.Value.ToString()) : c.Value)}";
            })));

            return this;
        }

        public IRequestContext AddQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(nameof(query));
            }

            switch (Uri.Query.Length)
            {
                case 0:
                    if ('?' != query[0])
                    {
                        query = "?" + query;
                    }

                    break;

                case 1:
                    if ('?' == query[0])
                    {
                        query = query.Substring(1);
                    }

                    break;

                default:
                    if ('&' != query[0])
                    {
                        query = "&" + query;
                    }

                    break;
            }

            _uri = new Uri($"{_uri.OriginalString}{query}", UriKind.RelativeOrAbsolute);

            return this;
        }

        public IRequestContext AddCookie(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            CookieContainer.Add(new Cookie(name, value, "/", Uri.Host));

            return this;
        }

        public IRequestContext AddCookies(IDictionary<string, string> cookies)
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

        public IRequestContext AddCookies(string cookieHeaders)
        {
            if (string.IsNullOrWhiteSpace(cookieHeaders))
            {
                throw new ArgumentException(nameof(cookieHeaders));
            }

            CookieContainer.SetCookies(Uri, cookieHeaders.Replace(";", ","));

            return this;
        }

        public IRequestContext AddCookies(CookieCollection cookies)
        {
            if (cookies is null)
            {
                throw new ArgumentNullException(nameof(cookies));
            }

            CookieContainer.Add(cookies);

            return this;
        }

        public IRequestContext SetUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                throw new ArgumentException(nameof(userAgent));
            }

            _httpRequestMessageAction += c =>
            {
                c.Headers.UserAgent.Clear();
                c.Headers.TryAddWithoutValidation("User-Agent", userAgent);
            };

            return this;
        }

        public IRequestContext SetAuthorization(string scheme, string parameter)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException(nameof(scheme));
            }

            var authentication = new AuthenticationHeaderValue(scheme, parameter);
            _httpRequestMessageAction += c => c.Headers.Authorization = authentication;

            return this;
        }

        public IRequestContext SetReferer(string referer)
        {
            if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(nameof(referer));
            }

            _httpRequestMessageAction += c => c.Headers.Referrer = uri;

            return this;
        }

        public IRequestContext SetContent(string content, string contentType = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(nameof(content));
            }

            return SetContent<string>(content, contentType);
        }

        public IRequestContext SetContent(IDictionary<string, string> content, string contentType = null)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return SetContent<IDictionary<string, string>>(content, contentType);
        }

        public IRequestContext SetContent(HttpContent content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return SetContent<HttpContent>(content);
        }

        public IRequestContext SetContent(FileContent content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return SetContent<FileContent>(content);
        }

        public IRequestContext SetContent<T>(T content, string contentType = null)
            where T : class
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            MediaTypeHeaderValue mediaTypeHeaderValue = null;
            if (null != contentType)
            {
                mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            }

            _httpContent = GetContent(content, mediaTypeHeaderValue);

            return this;
        }

        public IRequestContext SetRelativeUri(Uri uri)
        {
            Uri.TryCreate(_uri, uri, out _uri);

            return this;
        }

        public IRequestContext SetRelativeUri(string uri)
        {
            return SetRelativeUri(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        public IRequestContext SetContentType(string contentType)
        {
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

            _httpContentAction += c =>
            {
                if (null != c)
                {
                    c.Headers.ContentType = mediaTypeHeaderValue;
                }
            };

            return this;
        }

        public IRequestContext SetTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            _timeout = timeout;

            return this;
        }

        public IRequestContext SetJsonSerializerSettings(Action<JsonSerializerSettings> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (null == _jsonSerializerSettings)
            {
                _jsonSerializerSettings = new JsonSerializerSettings();
            }

            action(_jsonSerializerSettings);

            return this;
        }

        public IRequestContext SetEncoding(Encoding encoding)
        {
            _encoding = encoding;

            return this;
        }

        public IRequestContext SetRetry(int retryCount, Func<int, int> policy = null, RetryOptions retryOptions = RetryOptions.Timeout)
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            _retryCount = retryCount;
            _retryOptions = retryOptions;
            _retryPolicy = policy;

            return this;
        }

        public IRequestContext SetVersion(Version version)
        {
            if (version is null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            _version = version;

            return this;
        }

        public IRequestContext UseCookies(bool useCookies = true)
        {
            _httpMessageHandler.UseCookies = useCookies;

            return this;
        }

        public IRequestContext UseHttpCompletionOption(HttpCompletionOption httpCompletionOption)
        {
            _httpCompletionOption = httpCompletionOption;

            return this;
        }

        public IRequestContext EnsureSuccessStatusCode(bool isEnsureSuccessStatusCode = true)
        {
            _ensureSuccessStatusCode = isEnsureSuccessStatusCode;

            return this;
        }

        private HttpContent GetContent(object content, MediaTypeHeaderValue contentType = null)
        {
            if (null == content)
            {
                return null;
            }

            if (content is FileContent fileContent)
            {
                return fileContent.Build(_encoding);
            }

            if (content is HttpContent httpContent)
            {
                return httpContent;
            }

            if (_isFormData)
            {
                IDictionary<string, string> keyValuePairs;

                switch (content)
                {
                    case string str:
                        try
                        {
                            keyValuePairs = new Dictionary<string, string>();

                            foreach (var item in str.Split('&'))
                            {
                                var index = item.IndexOf('=');

                                keyValuePairs[item.Substring(0, index)] = item.Substring(index + 1);
                            }
                        }
                        catch
                        {
                            throw new ArgumentException(nameof(content));
                        }

                        break;

                    case IDictionary<string, string> dic:
                        keyValuePairs = dic;

                        break;

                    case IDictionary dictionary:
                        keyValuePairs = new Dictionary<string, string>();

                        foreach (DictionaryEntry entry in dictionary)
                        {
                            keyValuePairs[entry.Key?.ToString()] = entry.Value?.ToString();
                        }

                        break;

                    default:
                        keyValuePairs = new Dictionary<string, string>();

                        foreach (var property in content.GetType().GetProperties())
                        {
                            keyValuePairs[property.Name] = property.GetValue(content)?.ToString();
                        }

                        break;
                }

                return SetContentType(new FormUrlEncodedContent(keyValuePairs), contentType);
            }

            contentType = contentType ?? _httpUtilConfig.ContentTypeHeaderValue;

            switch (content)
            {
                case string str:
                    return SetContentType(new StringContent(str, _encoding ?? Encoding.Default), contentType);

                case Stream stream:
                    return SetContentType(new StreamContent(stream), contentType);

                default:
                    return SetContentType(new StringContent(JsonConvert.SerializeObject(content, _jsonSerializerSettings), _encoding ?? Encoding.Default), contentType);
            }
        }

        private static HttpContent SetContentType(HttpContent httpContent, MediaTypeHeaderValue contentType)
        {
            if (null != contentType)
            {
                httpContent.Headers.ContentType = contentType;
            }

            return httpContent;
        }

        private async Task<IResponseContext> SendAsync(int tryCount = 0, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(IRequestContext).Name);
            }

            var cancellationTokenSource = new CancellationTokenSource();
            var httpRequestMessage = new HttpRequestMessage(_httpMethod, _uri)
            {
                Version = _version,

#if NET7_0_OR_GREATER

                VersionPolicy = _httpClient.DefaultVersionPolicy

#endif
            };

            try
            {
                _pendingCounter.Start();
                httpRequestMessage.Content = _httpContent;
                _httpContentAction?.Invoke(httpRequestMessage.Content);
                _httpRequestMessageAction?.Invoke(httpRequestMessage);

                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);
                cancellationTokenSource.CancelAfter(_timeout);

                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, _httpCompletionOption, cancellationTokenSource.Token);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    var retryResponse = await RetryAsync(RetryOptions.FailStatusCode, tryCount, cancellationTokenSource.Token);
                    if (null != retryResponse)
                    {
                        return retryResponse;
                    }

                    if (_ensureSuccessStatusCode)
                    {
                        httpResponseMessage.EnsureSuccessStatusCode();
                    }
                }

                return new ResponseContext(this, httpResponseMessage);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellationTokenSource.IsCancellationRequested && cancellationTokenSource.Token == ex.CancellationToken)
                {
                    return await RetryAsync(RetryOptions.Timeout, tryCount, cancellationTokenSource.Token) ?? throw new TimeoutException($"The request was canceled due to the configured {nameof(IRequestContext)} or {nameof(HttpUtilConfig.Timeout)} of {_timeout.TotalSeconds} seconds elapsing.");
                }

                throw;
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch
            {
                var retryResponse = await RetryAsync(RetryOptions.Exception, tryCount, cancellationTokenSource.Token);
                if (null != retryResponse)
                {
                    return retryResponse;
                }

                throw;
            }
            finally
            {
                httpRequestMessage.Dispose();

                _pendingCounter.Completed();
            }
        }

        public async Task<IResponseContext> SendAsync(CancellationToken cancellationToken = default)
        {
            return await SendAsync(0, cancellationToken);
        }

        private async Task<IResponseContext> RetryAsync(RetryOptions retryOptions, int tryCount, CancellationToken cancellationToken)
        {
            if (!_disposed && (_retryOptions & retryOptions) > 0 && tryCount++ < _retryCount)
            {
                if (null != _retryPolicy)
                {
                    var dealy = _retryPolicy(tryCount);
                    if (dealy > 0)
                    {
                        await Task.Delay(dealy, cancellationToken);
                    }
                }

                if (!_disposed)
                {
                    return await SendAsync(tryCount, cancellationToken);
                }
            }

            return null;
        }

        public async Task<T> ReadAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return await (await SendAsync(cancellationToken)).ReadAsAsync<T>(cancellationToken);
        }

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return await (await SendAsync(cancellationToken)).ReadAsStringAsync(cancellationToken);
        }

        public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return await (await SendAsync(cancellationToken)).ReadAsStreamAsync(cancellationToken);
        }

#if NET7_0_OR_GREATER

        /// <summary>
        /// Reads the HTTP content and returns the value that results from deserializing the content as JSON in an async enumerable operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ReadAsAsyncEnumerable<T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in (await SendAsync(cancellationToken)).ReadAsAsyncEnumerable<T>(cancellationToken))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Reads the HTTP content and returns the value that results from deserializing the content as JSON in an async enumerable operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ReadStreamAsAsyncEnumerable<T>(AsyncEnumerableOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            await foreach (var item in (await SendAsync(cancellationToken)).ReadStreamAsAsyncEnumerable<T>(options, cancellationToken))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Reads the HTTP content and returns the value that results from deserializing the content as JSON in an async enumerable operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ReadStreamAsAsyncEnumerable<T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in (await SendAsync(cancellationToken)).ReadStreamAsAsyncEnumerable<T>(cancellationToken))
            {
                yield return item;
            }
        }

#endif

        public async Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return await (await SendAsync(cancellationToken)).ReadAsByteArrayAsync(cancellationToken);
        }

        public async Task<string> ReadAsBase64StringAsync(Base64FormattingOptions options = Base64FormattingOptions.None, CancellationToken cancellationToken = default)
        {
            return await (await SendAsync(cancellationToken)).ReadAsBase64StringAsync(options, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            using (var httpRequestMessage = new HttpRequestMessage(_httpMethod, Uri))
            {
                _httpRequestMessageAction?.Invoke(httpRequestMessage);

                if (null != _httpContent)
                {
                    httpRequestMessage.Content = new StringContent(string.Empty);
                    _httpContentAction(httpRequestMessage.Content);

                    foreach (var header in _httpContent.Headers)
                    {
                        httpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                return Regex.Replace(httpRequestMessage.ToString(), @"Content: .+, ?", string.Empty);
            }
        }
    }
}