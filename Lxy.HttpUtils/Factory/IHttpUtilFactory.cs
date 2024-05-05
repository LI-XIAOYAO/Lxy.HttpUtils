using System;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// Provide an <see cref="IHttpUtil"/> factory.
    /// </summary>
    public interface IHttpUtilFactory : IDisposable
    {
        /// <summary>
        /// Get an <see cref="IHttpUtil"/> by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IHttpUtil Get(string name);

        /// <summary>
        /// Get an <see cref="IHttpUtil"/>.
        /// </summary>
        /// <returns></returns>
        IHttpUtil Get();
    }
}