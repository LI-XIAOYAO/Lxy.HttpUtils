using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// DefaultHttpUtilFactory
    /// </summary>
    internal class DefaultHttpUtilFactory : IHttpUtilFactory
    {
        private bool _disposed;
        private static readonly Timer _timer = new Timer();
        private static readonly object _lock = new object();
        private static ConcurrentDictionary<string, IHttpUtil> HttpUtilContainer { get; } = new ConcurrentDictionary<string, IHttpUtil>();
        internal static ConcurrentDictionary<string, HttpUtilConfig> HttpUtilConfigs { get; } = new ConcurrentDictionary<string, HttpUtilConfig>();
        internal static HttpUtilConfig HttpUtilConfig { get; } = new HttpUtilConfig();

        static DefaultHttpUtilFactory()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HttpUtilFactoryActiveHandler();
        }

        public IHttpUtil Get(string name)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!HttpUtilContainer.TryGetValue(name, out var httpUtil))
            {
                lock (HttpUtilContainer)
                {
                    if (!HttpUtilContainer.TryGetValue(name, out httpUtil))
                    {
                        HttpUtilContainer.TryAdd(name, httpUtil = GetHttpUtil(name));
                    }
                }
            }

            return httpUtil;
        }

        public IHttpUtil Get()
        {
            return GetHttpUtil(HttpUtilConfig);
        }

        private static DefaultHttpUtil GetHttpUtil(string name)
        {
            var httpUtil = GetHttpUtil(HttpUtilConfigs.TryGetValue(name, out var config) ? config : HttpUtilConfig);
            httpUtil.OnDisposed += (s, a) => HttpUtilContainer.TryRemove(name, out var _);

            return httpUtil;
        }

        private static DefaultHttpUtil GetHttpUtil(HttpUtilConfig httpUtilConfig)
        {
            return new DefaultHttpUtil(httpUtilConfig)
            {
                LastCallTime = DateTime.Now
            };
        }

        private static void HttpUtilFactoryActiveHandler()
        {
            var interval = TimeSpan.FromMinutes(3);

            _timer.Interval = interval.TotalMilliseconds;
            _timer.Elapsed += (s, e) =>
            {
#if DEBUG
                Debug.WriteLine($"[{interval}]Timer runing...", nameof(HttpUtilFactoryActiveHandler));
#endif
                if (!HttpUtilContainer.IsEmpty)
                {
                    lock (_lock)
                    {
                        if (!HttpUtilContainer.IsEmpty)
                        {
                            foreach (KeyValuePair<string, IHttpUtil> item in HttpUtilContainer)
                            {
                                var defaultHttpUtil = (DefaultHttpUtil)item.Value;

                                if (defaultHttpUtil.AutoDisposed.HasValue && DateTime.Now - defaultHttpUtil.LastCallTime > defaultHttpUtil.AutoDisposed)
                                {
                                    if (!defaultHttpUtil.PendingCounter.IsActive)
                                    {
                                        if (HttpUtilContainer.TryRemove(item.Key, out var httpUtil))
                                        {
                                            try
                                            {
                                                httpUtil.Dispose();
                                            }
#if DEBUG
                                            catch (Exception ex)
                                            {
                                                Debug.WriteLine($"[{item.Key}] {ex.Message}", nameof(HttpUtilFactoryActiveHandler));
                                            }
#endif
                                            finally
                                            {
                                            }

#if DEBUG
                                            Debug.WriteLine($"[{item.Key}] removed", nameof(HttpUtilFactoryActiveHandler));
#endif
                                        }
                                    }
#if DEBUG
                                    else
                                    {
                                        Debug.WriteLine($"[{item.Key}] runing...", nameof(HttpUtilFactoryActiveHandler));
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            };
            _timer.Start();
        }

        public static void Dispose(string name)
        {
            if (HttpUtilContainer.TryRemove(name, out var httpUtil))
            {
                httpUtil.Dispose();
            }
        }

        public static void Dispose(Uri uri)
        {
            Dispose(uri.Host);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _timer.Close();

                foreach (var item in HttpUtilContainer)
                {
                    Dispose(item.Key);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}