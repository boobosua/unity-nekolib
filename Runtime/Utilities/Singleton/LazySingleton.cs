using System;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Utilities
{
    /// <summary>
    /// Lazy singleton automatically creates a new singleton game object when a script uses it.
    /// Does not work if you have multiple singleton objects of type T in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> s_lazyInstance = new(CreateSingleton);

        public static T Instance => s_lazyInstance.Value;

        private static T CreateSingleton()
        {
            var ownerObject = new GameObject($"{typeof(T).Name} (Singleton)");
            var instance = ownerObject.AddComponent<T>();
            DontDestroyOnLoad(ownerObject);

            Debug.Log($"Create a new singleton of type {typeof(T).Name.Colorize(Swatch.LA)}.");

            return instance;
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            //Optional function that prevents manual instantiation.
            UnityEditor.EditorUtility.DisplayDialog("LazySingletons should not be manually instantiated", "Lazy loaded Singletons will load when first accessed. Instances that are part of scenes or prefabs do nothing.", "Ok");
            DestroyImmediate(this);
        }
#endif
    }
}
