using Lxy.HttpUtils;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// HttpUtils dependency injection extension.
    /// </summary>
    public static class DependencyInjectionExtension
    {
        /// <summary>
        /// Adds the singleton <see cref="IHttpUtilFactory"/> and related services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static IServiceCollection AddHttpUtil(this IServiceCollection services, Action<HttpUtilConfig> config = null)
        {
            config?.Invoke(DefaultHttpUtilFactory.HttpUtilConfig);

            services.TryAddSingleton<IHttpUtilFactory, DefaultHttpUtilFactory>();

            return services;
        }

        /// <summary>
        /// Adds the singleton <see cref="IHttpUtilFactory"/> and related services to the <see cref="IServiceCollection"/> and configure a name.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public static IServiceCollection AddHttpUtil(this IServiceCollection services, string name, Action<HttpUtilConfig> config)
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
            config.Invoke(httpUtilConfig);

            services.TryAddSingleton<IHttpUtilFactory, DefaultHttpUtilFactory>();

            return services;
        }
    }
}