namespace Lxy.HttpUtils
{
    /// <summary>
    /// HTTP request failed retry options.
    /// </summary>
    public enum RetryOptions
    {
        /// <summary>
        /// Timeout
        /// </summary>
        Timeout = 1,

        /// <summary>
        /// Exception
        /// </summary>
        Exception = 2,

        /// <summary>
        /// FailStatusCode
        /// </summary>
        FailStatusCode = 4,

        /// <summary>
        /// All
        /// </summary>
        All = Timeout | Exception | FailStatusCode
    }
}