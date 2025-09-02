using System;
using UnityEngine.Events;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {

    }

    [Serializable] public class FloatEvent : UnityEvent<float> { }
    [Serializable] public class IntEvent : UnityEvent<int> { }
    [Serializable] public class StringEvent : UnityEvent<string> { }
    [Serializable] public class BoolEvent : UnityEvent<bool> { }
}
