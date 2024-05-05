using System;
using System.Threading;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// PendingCounter
    /// </summary>
    internal class PendingCounter
    {
        private int _value;

        public int Value => _value;

        /// <summary>
        /// OnStart
        /// </summary>
        public event EventHandler OnStart;

        /// <summary>
        /// OnCompleted
        /// </summary>
        public event EventHandler OnCompleted;

        /// <summary>
        /// PendingCounter
        /// </summary>
        /// <param name="value"></param>
        public PendingCounter(int value = 0)
        {
            _value = value;
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start()
        {
            Interlocked.Increment(ref _value);

            OnStart?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Completed
        /// </summary>
        public void Completed()
        {
            Interlocked.Decrement(ref _value);

            OnCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive => _value > 0;

        public override string ToString()
        {
            return $"Pending: {_value}";
        }
    }
}