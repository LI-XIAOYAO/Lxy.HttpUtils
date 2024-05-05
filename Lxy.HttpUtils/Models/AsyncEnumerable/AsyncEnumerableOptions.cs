using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
#if NET7_0_OR_GREATER

    /// <summary>
    /// HTTP content asynchronous enumerable options.
    /// </summary>
    public abstract class AsyncEnumerableOptions
    {
        /// <summary>
        /// Replaces data read from the content stream.
        /// </summary>
        public Func<string, string> Replacer { get; set; }

        /// <summary>
        /// Content stream reader, If null value is returned it will be skipped.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract ValueTask<string> Reader(StreamReader streamReader, CancellationToken cancellationToken);
    }

#endif
}