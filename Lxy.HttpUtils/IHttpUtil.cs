using System;
using System.Net;
using System.Net.Http;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// IHttpUtil
    /// </summary>
    public interface IHttpUtil : IDisposable
    {
        /// <summary>
        /// Gets the managed cookie container object.
        /// </summary>
        CookieContainer CookieContainer { get; }

        /// <summary>
        /// Send a <see cref="HttpMethod"/> request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        IRequestContext Method(Uri uri, HttpMethod httpMethod);

        /// <summary>
        /// Send a <see cref="HttpMethod"/> request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        IRequestContext Method(string uri, HttpMethod httpMethod);

        /// <summary>
        /// Send a POST request to the specified Uri.
        /// </summary>
        IRequestContext Post(Uri uri);

        /// <summary>
        /// Send a POST request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Post(string uri);

        /// <summary>
        /// Send a GET request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Get(Uri uri);

        /// <summary>
        /// Send a GET request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Get(string uri);

        /// <summary>
        /// Send a POST form-data request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext PostFormData(Uri uri);

        /// <summary>
        /// Send a POST form-data request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext PostFormData(string uri);

        /// <summary>
        /// Send a DELETE request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Delete(Uri uri);

        /// <summary>
        /// Send a DELETE request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Delete(string uri);

        /// <summary>
        /// Send a PUT request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Put(Uri uri);

        /// <summary>
        /// Send a PUT request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Put(string uri);

        /// <summary>
        /// Send a HEAD request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Head(Uri uri);

        /// <summary>
        /// Send a HEAD request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Head(string uri);

        /// <summary>
        /// Send a OPTIONS request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Options(Uri uri);

        /// <summary>
        /// Send a OPTIONS request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Options(string uri);

        /// <summary>
        /// Send a TRACE request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Trace(Uri uri);

        /// <summary>
        /// Send a TRACE request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Trace(string uri);

#if NET7_0_OR_GREATER

        /// <summary>
        /// Send a PATCH request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Patch(Uri uri);

        /// <summary>
        /// Send a PATCH request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Patch(string uri);

        /// <summary>
        /// Send a CONNECT request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Connect(Uri uri);

        /// <summary>
        /// Send a CONNECT request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRequestContext Connect(string uri);

#endif

        /// <summary>
        /// Cancel all pending requests on this instance.
        /// </summary>
        IHttpUtil CancelPendingRequests();
    }
}