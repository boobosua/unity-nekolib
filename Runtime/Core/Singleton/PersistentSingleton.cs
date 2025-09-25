using UnityEngine;
using NekoLib.Extensions;
using NekoLib.Utilities;

namespace NekoLib.Core
{
    /// <summary>
    /// DontDestroy singleton can be created and placed in a persistent scene and it will exist across scenes.
    /// Do not reference the instance in OnDestroy(), OnDisable() or OnApplicationQuit().
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        protected static bool s_applicationIsQuitting = false;
        private static bool s_isInitializing = false;

        public static bool HasInstance =>
            s_instance != null &&
            s_instance.gameObject != null &&
            !s_applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (s_applicationIsQuitting)
                {
                    Debug.LogWarning($"Instance {typeof(T).Name.Colorize(Swatch.GA)} already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                if (s_instance != null && s_instance.gameObject != null)
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
                    Debug.Log($"Create a new singleton of type {typeof(T).Name.Colorize(Swatch.LA)}.");
                }
                else
                {
                    // Debug.LogWarning($"Found multiple instances of {typeof(T).Name.Colorize(Palette.GoldenAmber)}. This is not allowed.");
                    s_instance = allInstances[0]; // Fallback to the first found instance.

                    for (int i = 1; i < allInstances.Length; i++)
                    {
                        Debug.LogWarning($"Destroyed duplicate instance of {allInstances[i].name.Colorize(Swatch.GA)}.");
                        Destroy(allInstances[i].gameObject);
                    }
                }

                s_isInitializing = false;
                return s_instance;
            }
        }

        protected virtual void Awake()
        {
            // Only construct if not initializing and no instance exists.
            if (s_instance == null && !s_isInitializing)
            {
                s_instance = this as T;
            }
            else if (s_instance != null && s_instance != this && !s_isInitializing)
            {
                // If there's already an instance and it's not this one, destroy this duplicate
                Debug.LogWarning($"Destroyed duplicate instance of {gameObject.name.Colorize(Swatch.GA)}.");
                Destroy(gameObject);
                return;
            }

            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (gameObject.name != $"{typeof(T).Name} (Singleton)")
                gameObject.name = $"{typeof(T).Name} (Singleton)";
        }

        private static void ResetFields()
        {
            s_instance = null;
            s_applicationIsQuitting = false;
            s_isInitializing = false;
        }
#endif

        protected virtual void OnApplicationQuit()
        {
            // Prevent others from referencing this instance when the application is quitting.
            s_applicationIsQuitting = true;

#if UNITY_EDITOR
            if (Utils.IsReloadDomainDisabled())
            {
                // If domain reload is disabled, we need to reset static fields manually.
                ResetFields();
                Debug.Log($"Resetting static fields of {typeof(T).Name.Colorize(Swatch.GA)} because domain reload is disabled.");
            }
#endif
        }
    }
}
