using System.Threading.Tasks;
using UnityEngine;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {

    }

    public class YieldTask : CustomYieldInstruction
    {
        public Task Task { get; }

        public YieldTask(Task task)
        {
            Task = task;
        }

        public override bool keepWaiting => !Task.IsCompleted;
    }
}
