using System;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Singleton
{
    /// <summary>
    /// Lazy singleton automatically creates a new singleton game object when a script uses it.
    /// Does not work if you have multiple singleton objects of type T in the scene.
    /// </summary>
    public abstract class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> s_lazyInstance = new(CreateSingleton);

        public static T Instance => s_lazyInstance.Value;

        private static T CreateSingleton()
        {
            var ownerObject = new GameObject($"{typeof(T).Name} (Singleton)");
            var instance = ownerObject.AddComponent<T>();
            DontDestroyOnLoad(ownerObject);

            Debug.Log($"Create a new singleton of type {typeof(T).Name.Colorize(Color.magenta)}.");

            return instance;
        }
    }
}
