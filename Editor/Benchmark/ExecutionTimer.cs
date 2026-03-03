#if UNITY_EDITOR
using System;
using System.Diagnostics;

namespace NekoLib.Benchmark
{
    /// <summary> A simple utility to measure execution time of a code block.</summary>
    public sealed class ExecutionTimer
    {
        private Stopwatch _sw;

        /// <summary>Starts timing immediately using <see cref="Stopwatch.StartNew"/>.</summary>
        public ExecutionTimer()
        {
            _sw = Stopwatch.StartNew();
        }

        /// <summary>Stops the timer and outputs the elapsed time in milliseconds.</summary>
        public void Stop(out double elapsedMilliseconds)
        {
            if (_sw == null) throw new InvalidOperationException("ExecutionTimer was not started.");

            _sw.Stop();
            elapsedMilliseconds = _sw.Elapsed.TotalMilliseconds;
            _sw = null;
        }
    }
}
#endif
