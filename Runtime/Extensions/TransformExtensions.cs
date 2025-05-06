using UnityEngine;

namespace NekoLib.Extensions
{
    public static class TransformExtensions
    {
        public static void DestroyChildren(this Transform transform)
        {
            while (transform.childCount > 0)
            {
                Transform child = transform.transform.GetChild(0);
                child.SetParent(null);
                Object.Destroy(child.gameObject);
            }
        }
    }
}

