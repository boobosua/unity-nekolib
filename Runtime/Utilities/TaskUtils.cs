using System.Threading.Tasks;
using UnityEngine;

namespace TRnK.Utilities
{
    public static partial class Utils
    {

    }

    internal class YieldTask : CustomYieldInstruction
    {
        public Task Task { get; }

        public YieldTask(Task task)
        {
            Task = task;
        }

        public override bool keepWaiting => !Task.IsCompleted;
    }
}
