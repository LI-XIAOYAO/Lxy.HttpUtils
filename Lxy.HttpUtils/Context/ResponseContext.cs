using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// ResponseContext
    /// </summary>
    internal class ResponseContext : IResponseContext
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private string _requestInfo;
        private string _responseInfo;
        private bool _disposed;

#if NET7_0_OR_GREATER

        private static readonly System.Text.Json.JsonSerializerOptions _jsonSerializerDefaults_Web = new(System.Text.Json.JsonSerializerDefaults.Web);

#endif

        public ResponseContext(IRequestContext requestContext, HttpResponseMessage httpResponseMessage)
        {
            RequestContext = requestContext;
            _httpResponseMessage = httpResponseMessage;
            StatusCode = httpResponseMessage.StatusCode;
            Version = httpResponseMessage.Version;
            ContentType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
            ContentLength = httpResponseMessage.Content.Headers.ContentLength;
            Headers = httpResponseMessage.Headers;
            Cookies = Headers.Where(c => c.Key.StartsWith("Set-Cookie", StringComparison.OrdinalIgnoreCase)).SelectMany(c => c.Value).ParseCookie();
            StatusCode = httpResponseMessage.StatusCode;
            IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
            ReasonPhrase = httpResponseMessage.ReasonPhrase;
            ContentHeaders = httpResponseMessage.Content.Headers;
        }

        public IRequestContext RequestContext { get; }

        public HttpStatusCode StatusCode { get; }

        public bool IsSuccessStatusCode { get; }

        public string ReasonPhrase { get; }

        public IReadOnlyList<Cookie> Cookies { get; }

        public HttpResponseHeaders Headers { get; }

        public HttpContentHeaders ContentHeaders { get; }

        public Version Version { get; }

        public string ContentType { get; }

        public long? ContentLength { get; }

        public IEnumerable<string> Allow => ContentHeaders.Allow;

        public string RequestInfo => _requestInfo ?? (_requestInfo = RequestContext.ToString());

        public string ResponseInfo => _responseInfo ?? (_responseInfo = Regex.Replace(_httpResponseMessage.ToString(), @"Content: .+, ?", string.Empty));

        public async Task<T> ReadAsAsync<T>(CancellationToken cancellationToken = default)
        {
            if (!ContentLength.HasValue || 0 == ContentLength)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(await ReadAsStringAsync(cancellationToken));
        }

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            using (this)
            {
#if NET7_0_OR_GREATER

                return await _httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

#else

                return await _httpResponseMessage.Content.ReadAsStringAsync();

#endif
            }
        }

        public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken = default)
        {
#if NET7_0_OR_GREATER

            return await _httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

#else

            return await _httpResponseMessage.Content.ReadAsStreamAsync();

#endif
        }

#if NET7_0_OR_GREATER

        public async IAsyncEnumerable<T> ReadAsAsyncEnumerable<T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var stream = await _httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            await foreach (var item in System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<T>(stream, _jsonSerializerDefaults_Web, cancellationToken))
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<T> ReadStreamAsAsyncEnumerable<T>(AsyncEnumerableOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            using var stream = await _httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            var streamReader = new StreamReader(stream);
            var isString = typeof(T) == typeof(string);

            while (!streamReader.EndOfStream)
            {
                var data = await options.ReadAsync(streamReader, cancellationToken);

                if (null == data)
                {
                    continue;
                }

                yield return isString ? (T)(object)data : JsonConvert.DeserializeObject<T>(data);
            }
        }

        public async IAsyncEnumerable<T> ReadStreamAsAsyncEnumerable<T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in ReadStreamAsAsyncEnumerable<T>(new LineAsyncEnumerableOptions { IgnoreEmptyLines = true }, cancellationToken))
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<Memory<byte>> ReadStreamAsAsyncEnumerable(BytesAsyncEnumerableOptions options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var stream = await _httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            await foreach (var bytes in (options ??= BytesAsyncEnumerableOptions.Default).ReaderBytesAsync(stream, cancellationToken))
            {
                yield return bytes;
            }
        }

#endif

        public async Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            using (this)
            {
#if NET7_0_OR_GREATER

                return await _httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);

#else

                return await _httpResponseMessage.Content.ReadAsByteArrayAsync();

#endif
            }
        }

        public async Task<string> ReadAsBase64StringAsync(Base64FormattingOptions options = Base64FormattingOptions.None, CancellationToken cancellationToken = default)
        {
            if (!ContentLength.HasValue || 0 == ContentLength)
            {
                return default;
            }

            return Convert.ToBase64String(await ReadAsByteArrayAsync(cancellationToken), options);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _httpResponseMessage.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString() => ResponseInfo;
    }
}