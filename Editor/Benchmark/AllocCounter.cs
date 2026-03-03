#if UNITY_EDITOR
using System;
using NekoLib.Logger;
using UnityEngine.Profiling;

namespace NekoLib.Benchmark
{
    /// <summary> Utility class to count GC allocations during a specific code block. </summary>
    public sealed class AllocCounter
    {
        private readonly string _label;
        private readonly long _startMemory;
        private Recorder _rec;

        public AllocCounter(string label)
        {
            _label = label;

            _rec = Recorder.Get("GC.Alloc");
            _rec.enabled = false;

#if !UNITY_WEBGL
            _rec.FilterToCurrentThread();
#endif

            _rec.enabled = true;

            _startMemory = GC.GetAllocatedBytesForCurrentThread();
        }

        ///<summary> Stops the counter and logs the results. </summary>
        public void Stop()
        {
            if (_rec == null) throw new InvalidOperationException("AllocCounter already stopped.");

            _rec.enabled = false;

#if !UNITY_WEBGL
            _rec.CollectFromAllThreads();
#endif

            int allocCount = _rec.sampleBlockCount;
            long bytes = GC.GetAllocatedBytesForCurrentThread() - _startMemory;
            _rec = null;

            Log.Info($"[Benchmark] {_label} — {allocCount} GC alloc(s), {FormatBytes(bytes)} allocated");
        }

        /// <summary> Instruments the given action and logs the GC allocations made during its execution. </summary>
        public static void Instrument(string label, Action action)
        {
            var counter = new AllocCounter(label);

            try
            {
                action();
            }
            finally
            {
                counter.Stop();
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1_048_576) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / 1_048_576.0:F2} MB";
        }
    }
}
#endif