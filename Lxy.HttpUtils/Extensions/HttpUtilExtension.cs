using System;
using System.Net.Http;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// Send an HTTP request.
    /// </summary>
    public static class HttpUtil
    {
        private static readonly DefaultHttpUtilFactory _defaultHttpUtilFactory = new DefaultHttpUtilFactory();

        /// <summary>
        /// Set default config.
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetConfig(Action<HttpUtilConfig> config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config(DefaultHttpUtilFactory.HttpUtilConfig);
        }

        /// <summary>
        /// Set HttpUtil with a specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetConfig(this string name, Action<HttpUtilConfig> config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var httpUtilConfig = DefaultHttpUtilFactory.HttpUtilConfigs.GetOrAdd(name, (HttpUtilConfig)DefaultHttpUtilFactory.HttpUtilConfig.Clone());
            config(httpUtilConfig);
        }

        /// <summary>
        /// Set the <see cref="Uri.Host"/> to specified name in HttpUtil.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetConfig(this Uri uri, Action<HttpUtilConfig> config)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            SetConfig(uri.RelativeUri().Host, config);
        }

        /// <summary>
        /// Get default HttpUtil.
        /// </summary>
        /// <returns></returns>
        public static IHttpUtil GetHttpUtil()
        {
            return _defaultHttpUtilFactory.Get();
        }

        /// <summary>
        /// Get HttpUtil by specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IHttpUtil GetHttpUtil(this string name)
        {
            return _defaultHttpUtilFactory.Get(name);
        }

        /// <summary>
        /// Get HttpUtil by <see cref="Uri.Host"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IHttpUtil GetHttpUtil(this Uri uri)
        {
            return _defaultHttpUtilFactory.Get(uri.RelativeUri().Host);
        }

        /// <summary>
        /// Get HttpUtil by specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IHttpUtil GetHttpUtil(this string name, Action<HttpUtilConfig> config)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var httpUtilConfig = DefaultHttpUtilFactory.HttpUtilConfigs.GetOrAdd(name, (HttpUtilConfig)DefaultHttpUtilFactory.HttpUtilConfig.Clone());
            config(httpUtilConfig);

            return _defaultHttpUtilFactory.Get(name);
        }

        /// <summary>
        /// Get HttpUtil by <see cref="Uri.Host"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IHttpUtil GetHttpUtil(this Uri uri, Action<HttpUtilConfig> config)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return GetHttpUtil(uri.RelativeUri().Host, config);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Method(Uri, HttpMethod)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static IRequestContext Method(this Uri uri, HttpMethod httpMethod)
        {
            return GetHttpUtil(uri).Method(uri, httpMethod);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Method(string, HttpMethod)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static IRequestContext Method(this string uri, HttpMethod httpMethod)
        {
            return GetHttpUtil(new Uri(uri, UriKind.RelativeOrAbsolute)).Method(uri, httpMethod);
        }

        /// <summary>
        /// Send a POST request to the specified Uri.
        /// </summary>
        public static IRequestContext Post(this Uri uri)
        {
            return uri.Method(HttpMethod.Post);
        }

        /// <summary>
        /// Send a POST request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Post(this string uri)
        {
            return uri.Method(HttpMethod.Post);
        }

        /// <summary>
        /// Send a GET request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Get(this Uri uri)
        {
            return uri.Method(HttpMethod.Get);
        }

        /// <summary>
        /// Send a GET request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Get(this string uri)
        {
            return uri.Method(HttpMethod.Get);
        }

        /// <summary>
        /// Send a POST form-data request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext PostFormData(this Uri uri)
        {
            return GetHttpUtil(uri).PostFormData(uri);
        }

        /// <summary>
        /// Send a POST form-data request to the specified Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext PostFormData(this string uri)
        {
            return new Uri(uri, UriKind.RelativeOrAbsolute).PostFormData();
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Delete(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Delete(this Uri uri)
        {
            return Method(uri, HttpMethod.Delete);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Delete(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Delete(this string uri)
        {
            return Method(uri, HttpMethod.Delete);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Put(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Put(this Uri uri)
        {
            return Method(uri, HttpMethod.Put);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Put(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Put(this string uri)
        {
            return Method(uri, HttpMethod.Put);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Head(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Head(this Uri uri)
        {
            return Method(uri, HttpMethod.Head);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Head(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Head(this string uri)
        {
            return Method(uri, HttpMethod.Head);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Options(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Options(this Uri uri)
        {
            return Method(uri, HttpMethod.Options);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Options(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Options(this string uri)
        {
            return Method(uri, HttpMethod.Options);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Trace(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Trace(this Uri uri)
        {
            return Method(uri, HttpMethod.Trace);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Trace(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Trace(this string uri)
        {
            return Method(uri, HttpMethod.Trace);
        }

#if NET7_0_OR_GREATER

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Patch(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Patch(this Uri uri)
        {
            return Method(uri, HttpMethod.Patch);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Patch(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Patch(this string uri)
        {
            return Method(uri, HttpMethod.Patch);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Connect(Uri)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Connect(this Uri uri)
        {
            return Method(uri, HttpMethod.Connect);
        }

        /// <summary>
        /// <inheritdoc cref="IHttpUtil.Connect(string)"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IRequestContext Connect(this string uri)
        {
            return Method(uri, HttpMethod.Connect);
        }

#endif

        /// <summary>
        /// RelativeUri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Uri RelativeUri(this Uri uri)
        {
            if (uri is null || (!uri.IsAbsoluteUri && (DefaultHttpUtilFactory.HttpUtilConfig.BaseAddress is null)))
            {
                throw new ArgumentException(uri.OriginalString, nameof(uri));
            }

            if (!uri.IsAbsoluteUri)
            {
                uri = new Uri(DefaultHttpUtilFactory.HttpUtilConfig.BaseAddress, uri);
            }

            return uri;
        }

        /// <summary>
        /// Dispose <see cref="IHttpUtil"/> by <see cref="Uri.Host"/>.
        /// </summary>
        /// <param name="uri"></param>
        public static void Dispose(this Uri uri)
        {
            DefaultHttpUtilFactory.Dispose(uri.RelativeUri());
        }

        /// <summary>
        /// Dispose <see cref="IHttpUtil"/> by specified name.
        /// </summary>
        /// <param name="name"></param>
        public static void Dispose(this string name)
        {
            DefaultHttpUtilFactory.Dispose(name);
        }
    }
}