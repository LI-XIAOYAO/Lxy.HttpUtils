using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lxy.HttpUtils
{
#if NET7_0_OR_GREATER

    /// <summary>
    /// HTTP content asynchronous stream block processing options.
    /// </summary>
    public class BlockAsyncEnumerableOptions : AsyncEnumerableOptions
    {
        private Memory<char> _memory = new char[8];
        private int _size = 8;

        /// <summary>
        /// Read bytes size, default 8 bytes.
        /// </summary>
        public int Size
        {
            get => _size;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

                _size = value;
                _memory = new char[_size];
            }
        }

        /// <summary>
        /// Read 8 bytes at once.
        /// </summary>
        public static BlockAsyncEnumerableOptions Default => new();

        /// <summary>
        /// <inheritdoc cref="BlockAsyncEnumerableOptions"/>
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static BlockAsyncEnumerableOptions New(int size) => new() { Size = size };

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="streamReader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async ValueTask<string> ReadAsync(StreamReader streamReader, CancellationToken cancellationToken)
        {
            var read = await streamReader.ReadBlockAsync(_memory, cancellationToken);
            if (0 == read)
            {
                return null;
            }

            var data = _memory[..read].ToString();

            if (null != Replacer)
            {
                data = Replacer(data);
            }

            return data;
        }
    }

#endif
}