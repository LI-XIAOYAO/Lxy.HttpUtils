using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
#if NET7_0_OR_GREATER

    /// <summary>
    /// HTTP content asynchronous stream line processing options.
    /// </summary>
    public class LineAsyncEnumerableOptions : AsyncEnumerableOptions
    {
        /// <summary>
        /// Ignore empty lines, default false.
        /// </summary>
        public bool IgnoreEmptyLines { get; set; }

        /// <summary>
        /// <inheritdoc cref="LineAsyncEnumerableOptions"/>
        /// </summary>
        public static LineAsyncEnumerableOptions Default => new();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="streamReader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async ValueTask<string> ReadAsync(StreamReader streamReader, CancellationToken cancellationToken)
        {
            var data = await streamReader.ReadLineAsync(cancellationToken);

            if (null != Replacer)
            {
                data = Replacer(data);
            }

            if (null == data || (IgnoreEmptyLines && string.IsNullOrWhiteSpace(data)))
            {
                return null;
            }

            return data;
        }
    }

#endif
}