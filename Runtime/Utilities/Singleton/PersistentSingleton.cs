using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Singleton
{
    /// <summary>
    /// DontDestroy singleton can be created and placed in a persistent scene and it will exist across scenes.
    /// Do not reference the instance in OnDestroy(), OnDisable() or OnApplicationQuit().
    /// </summary>
    /// <typeparam name="T"> The type of the singleton. </typeparam>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static readonly object _lock = new();
        private static bool s_applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (s_applicationIsQuitting)
                    {
                        Debug.LogWarning($"Instance {typeof(T).Name.Colorize(Color.magenta)} already destroyed on application quit. Won't create again - returning null.");
                        return null;
                    }

                    if (s_instance == null)
                    {
                        var obj = new GameObject
                        {
                            name = $"{typeof(T).Name} (Singleton)"
                        };

                        s_instance = obj.AddComponent<T>();
                        Debug.Log($"Create a new singleton of type {typeof(T).Name.Colorize(Color.magenta)}.");
                    }
                }

                return s_instance;
            }
        }

        protected virtual void Awake()
        {
            if (s_instance == null)
            {
                s_instance = this as T;
            }
            else
            {
                if (s_instance != this)
                {
                    Debug.LogWarning($"Destroyed a duplicate {gameObject.name.Colorize(Color.magenta)} ({gameObject.GetInstanceID()}).");
                    Destroy(gameObject);
                    return;
                }
            }

            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (gameObject.name != $"{typeof(T).Name} (Singleton {gameObject.GetInstanceID()})")
                gameObject.name = $"{typeof(T).Name} (Singleton {gameObject.GetInstanceID()})";
        }
#endif

        protected virtual void OnApplicationQuit()
        {
            s_applicationIsQuitting = true;
            s_instance = null;
        }
    }
}
