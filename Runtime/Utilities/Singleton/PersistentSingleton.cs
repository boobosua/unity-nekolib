using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Utilities
{
    /// <summary>
    /// DontDestroy singleton can be created and placed in a persistent scene and it will exist across scenes.
    /// Do not reference the instance in OnDestroy(), OnDisable() or OnApplicationQuit().
    /// </summary>
    /// <typeparam name="T"> The type of the singleton. </typeparam>
    [DisallowMultipleComponent]
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static readonly object InstanceLock = new();
        private static bool s_applicationIsQuitting = false;
        private static bool s_isInitializing = false;

        public static T Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (s_applicationIsQuitting)
                    {
                        Debug.LogWarning($"Instance {typeof(T).Name.Colorize(Palette.GoldenAmber)} already destroyed on application quit. Won't create again - returning null.");
                        return null;
                    }

                    if (s_instance != null)
                    {
                        // Debug.Log($"Returning existing instance of {typeof(T).Name.Colorize(Palette.Lavender)}.");
                        return s_instance;
                    }

                    s_isInitializing = true;

                    var allInstances = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    // If there's exactly one instance, use it.
                    if (allInstances.Length == 1)
                    {
                        s_instance = allInstances[0];
                        // Debug.Log($"Using existing instance of {typeof(T).Name.Colorize(Palette.Lavender)}.");
                    }
                    else if (allInstances.Length == 0)
                    {
                        // Create a new GameObject for the singleton.
                        var obj = new GameObject
                        {
                            name = $"{typeof(T).Name} (Singleton)"
                        };

                        s_instance = obj.AddComponent<T>();
                        Debug.Log($"Create a new singleton of type {typeof(T).Name.Colorize(Palette.Lavender)}.");
                    }
                    else
                    {
                        // Debug.LogWarning($"Found multiple instances of {typeof(T).Name.Colorize(Palette.GoldenAmber)}. This is not allowed.");
                        s_instance = allInstances[0]; // Fallback to the first found instance.

                        for (int i = 1; i < allInstances.Length; i++)
                        {
                            Debug.LogWarning($"Destroyed duplicate instance of {allInstances[i].name.Colorize(Palette.GoldenAmber)}.");
                            Destroy(allInstances[i].gameObject);
                        }
                    }

                    s_isInitializing = false;
                    return s_instance;
                }
            }
        }

        protected virtual void Awake()
        {
            lock (InstanceLock)
            {
                // Only construct if not initializing and no instance exists.
                if (s_instance == null && !s_isInitializing)
                {
                    s_instance = this as T;
                }
                else if (s_instance != null && s_instance != this && !s_isInitializing)
                {
                    // If there's already an instance and it's not this one, destroy this duplicate
                    Debug.LogWarning($"Destroyed duplicate instance of {gameObject.name.Colorize(Palette.GoldenAmber)}.");
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void OnBeforeSceneLoad()
        {
            s_applicationIsQuitting = false;
        }

        protected virtual void OnApplicationQuit()
        {
            s_applicationIsQuitting = true;
        }
    }
}
