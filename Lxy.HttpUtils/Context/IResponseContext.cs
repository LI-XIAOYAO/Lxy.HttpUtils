using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// HTTP response context.
    /// </summary>
    public interface IResponseContext : IDisposable
    {
        /// <summary>
        /// HTTP request context.
        /// </summary>
        IRequestContext RequestContext { get; }

        /// <summary>
        ///  Gets the status code of the HTTP response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// <inheritdoc cref="HttpResponseMessage.IsSuccessStatusCode"/>
        /// </summary>
        bool IsSuccessStatusCode { get; }

        /// <summary>
        /// Gets the reason phrase which typically is sent by servers together with the status code.
        /// </summary>
        string ReasonPhrase { get; }

        /// <summary>
        /// HTTP response header 'Set-Cookie'.
        /// </summary>
        IReadOnlyList<Cookie> Cookies { get; }

        /// <summary>
        /// <inheritdoc cref="HttpResponseMessage.Headers"/>
        /// </summary>
        HttpResponseHeaders Headers { get; }

        /// <summary>
        /// <inheritdoc cref="HttpContent.Headers"/>
        /// </summary>
        HttpContentHeaders ContentHeaders { get; }

        /// <summary>
        /// Gets the HTTP message version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the value of the Content-Type content header on an HTTP response.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the value of the Content-Length content header on an HTTP response.
        /// </summary>
        long? ContentLength { get; }

        /// <summary>
        /// <inheritdoc cref="HttpContentHeaders.Allow"/>
        /// </summary>
        IEnumerable<string> Allow { get; }

        /// <summary>
        /// Request info.
        /// </summary>
        string RequestInfo { get; }

        /// <summary>
        /// Response info.
        /// </summary>
        string ResponseInfo { get; }

        /// <summary>
        /// Serialize the HTTP content to a <typeparamref name="T"/> as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ReadAsAsync<T>(CancellationToken cancellationToken = default);

        /// <summary>
        /// <inheritdoc cref="HttpContent.ReadAsStringAsync()"/>
        /// </summary>
        /// <returns></returns>
        Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// <inheritdoc cref="HttpContent.ReadAsStreamAsync()"/>
        /// </summary>
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