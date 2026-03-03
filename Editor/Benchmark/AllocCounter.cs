#if UNITY_EDITOR
using System;
using UnityEngine.Profiling;

namespace NekoLib.Benchmark
{
    /// <summary> A simple utility to count GC allocations in a code block. </summary>
    public sealed class AllocCounter
    {
        private Recorder _rec;

        /// <summary> Starts the allocation counter. </summary>
        public AllocCounter()
        {
            _rec = Recorder.Get("GC.Alloc");
            _rec.enabled = false;

#if !UNITY_WEBGL
            _rec.FilterToCurrentThread();
#endif
            _rec.enabled = true;
        }

        /// <summary> Stops the counter and outputs the number of allocations. </summary>
        public void Stop(out int allocCount)
        {
            if (_rec == null) throw new InvalidOperationException("AllocCounter was not started.");

            _rec.enabled = false;

#if !UNITY_WEBGL
            _rec.CollectFromAllThreads();
#endif

            var result = _rec.sampleBlockCount;
            _rec = null;

            allocCount = result;
        }
    }
}
#endif