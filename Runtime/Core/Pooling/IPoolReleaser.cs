using UnityEngine;

namespace NekoLib.Pooling
{
    internal interface IPoolReleaser
    {
        bool IsValid { get; }
        void Despawn(Component instance);
    }
}


