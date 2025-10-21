using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lxy.HttpUtils
{
#if NET7_0_OR_GREATER

    /// <summary>
    /// HTTP content asynchronous stream bytes processing options.
    /// </summary>
    public class BytesAsyncEnumerableOptions
    {
        private Memory<byte> _memory = new byte[8192];
        private int _size = 8192;

        /// <summary>
        /// Read bytes size, default 8192.
        /// </summary>
        public int Size
        {
            get => _size;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

                _size = value;
                _memory = new byte[_size];
            }
        }

        /// <summary>
        /// Read 8k at once.
        /// </summary>
        public static BytesAsyncEnumerableOptions Default => new();

        /// <summary>
        /// <inheritdoc cref="BytesAsyncEnumerableOptions"/>
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static BytesAsyncEnumerableOptions New(int size) => new() { Size = size };

        /// <summary>
        /// Read bytes from stream asynchronously.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Memory<byte>> ReaderBytesAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var read = 0;

            while ((read = await stream.ReadAsync(_memory, cancellationToken)) > 0)
            {
                yield return _memory[..read];
            }
        }
    }

#endif
}