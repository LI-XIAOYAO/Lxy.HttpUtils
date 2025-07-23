using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// HTTP request context.
    /// </summary>
    public interface IRequestContext : IDisposable
    {
        /// <summary>
        /// Gets the managed cookie container object.
        /// </summary>
        CookieContainer CookieContainer { get; }

        /// <summary>
        /// Adds the specified header and its value into the <see cref="HttpRequestMessage.Headers"/> collection.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext AddHeader(string name, string value);

        /// <summary>
        /// Adds the specified header and its value into the <see cref="HttpRequestMessage.Headers"/> collection.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext AddHeaders(IDictionary<string, string> headers);

        /// <summary>
        /// Adds a query string with a single given parameter name and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="isEscape"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext AddQuery(string name, object value, bool isEscape = true);

        /// <summary>
        /// Adds a query string composed from the given name value pairs.
        /// </summary>
        /// <param name="querys"></param>
        /// <param name="isEscape"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext AddQuery(IDictionary<string, object> querys, bool isEscape = true);

        /// <summary>
        /// Adds a query string.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext AddQuery(string query);

        /// <summary>
        /// Adds cookie to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext AddCookie(string name, string value);

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext AddCookies(IDictionary<string, string> cookies);

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookieHeaders"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext AddCookies(string cookieHeaders);

        /// <summary>
        /// Adds cookies to <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext AddCookies(CookieCollection cookies);

        /// <summary>
        /// Sets the value of the User-Agent header for HTTP request.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext SetUserAgent(string userAgent);

        /// <summary>
        /// Sets the value of the Authorization header for HTTP request.
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext SetAuthorization(string scheme, string parameter);

        /// <summary>
        /// Sets the content of the HTTP message.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext SetContent(string content, string contentType = null);

        /// <summary>
        /// Sets the content of the HTTP message.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetContent(IDictionary<string, string> content, string contentType = null);

        /// <summary>
        /// Sets the content of the HTTP message.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetContent(HttpContent content);

        /// <summary>
        /// Sets the file content of the HTTP message.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetContent(FileContent content);

        /// <summary>
        /// Sets the object of the HTTP message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetContent<T>(T content, string contentType = null)
            where T : class;

        /// <summary>
        /// Set relative or absolute URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetRelativeUri(Uri uri);

        /// <summary>
        /// Sets relative or absolute URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext SetRelativeUri(string uri);

        /// <summary>
        /// Sets the value of the Content-Type header for HTTP request.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        IRequestContext SetContentType(string contentType);

        /// <summary>
        /// Sets the timespan to wait before the request times out.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        IRequestContext SetTimeout(TimeSpan timeout);

        /// <summary>
        /// Sets request HTTP Content serializer.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetJsonSerializerSettings(Action<JsonSerializerSettings> action);

        /// <summary>
        /// Sets HTTP Content encoding.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        IRequestContext SetEncoding(Encoding encoding);

        /// <summary>
        /// Sets retry when failed, default <see cref="RetryOptions.Timeout" />.
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="policy"><![CDATA[Func<Retry, MillisecondsDelay>]]></param>
        /// <param name="retryOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        IRequestContext SetRetry(int retryCount, Func<int, int> policy = null, RetryOptions retryOptions = RetryOptions.Timeout);

        /// <summary>
        /// Sets the HTTP message version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IRequestContext SetVersion(Version version);

        /// <summary>
        /// Whether cookies should be used.
        /// </summary>
        /// <param name="useCookies"></param>
        /// <returns></returns>
        IRequestContext UseCookies(bool useCookies);

        /// <summary>
        /// When the operation should complete(as soon as a response is available or after reading the whole response content).
        /// </summary>
        /// <param name="httpCompletionOption"></param>
        /// <returns></returns>
        IRequestContext UseHttpCompletionOption(HttpCompletionOption httpCompletionOption);

        /// <summary>
        /// <inheritdoc cref="HttpResponseMessage.EnsureSuccessStatusCode()"/>
        /// </summary>
        /// <param name="isEnsureSuccessStatusCode"></param>
        /// <returns></returns>
        IRequestContext EnsureSuccessStatusCode(bool isEnsureSuccessStatusCode = true);

        /// <summary>
        /// <inheritdoc cref="HttpClient.SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IResponseContext> SendAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// <inheritdoc cref="HttpContent.ReadAsStringAsync()"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Serialize the HTTP content to a <typeparamref name="T"/> as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ReadAsAsync<T>(CancellationToken cancellationToken = default);

        /// <summary>
        /// <inheritdoc cref="HttpContent.ReadAsStreamAsync()"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken = default);

#if NET7_0_OR_GREATER

        /// <summary>
        /// Reads HTTP content and returns an asynchronous enumerable deserializing object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<T> ReadAsAsyncEnumerable<T>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads HTTP content and returns an asynchronous enumerable deserializing object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<T> ReadStreamAsAsyncEnumerable<T>(AsyncEnumerableOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads HTTP content and returns an asynchronous enumerable deserializing object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<T> ReadStreamAsAsyncEnumerable<T>(CancellationToken cancellationToken = default);

#endif

        /// <summary>
        /// <inheritdoc cref="HttpContent.ReadAsByteArrayAsync()"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Serialize the HTTP content to a base64 string as an asynchronous operation.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> ReadAsBase64StringAsync(Base64FormattingOptions options = Base64FormattingOptions.None, CancellationToken cancellationToken = default);
    }
}