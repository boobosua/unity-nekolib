using UnityEngine;
using NekoLib.Extensions;
using NekoLib.ColorPalette;

namespace NekoLib.Singleton
{
    /// <summary>
    /// Singleton that will be destroyed when the scene is unloaded.
    /// Can only be used within a scene.
    /// Do not reference the instance in OnDestroy(), OnDisable() or OnApplicationQuit().
    /// </summary>
    /// <typeparam name="T"> The type of the singleton. </typeparam>
    public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static readonly object s_lock = new();
        private static bool s_queueForDestroy = false;

        public static T Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (s_queueForDestroy)
                    {
                        Debug.LogWarning($"Instance {typeof(T).Name.Colorize(Palette.GoldenAmber)} already queued for destroy. Won't create again - returning null.");
                        return null;
                    }

                    if (s_instance == null)
                    {
                        s_instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                        if (s_instance == null)
                        {
                            var obj = new GameObject
                            {
                                name = $"{typeof(T).Name} (Singleton)"
                            };

                            s_instance = obj.AddComponent<T>();
                            Debug.Log($"Create a new singleton of type {typeof(T).Name.Colorize(Palette.Lavender)}.");
                        }
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
                    Debug.LogWarning($"Destroyed a duplicate {gameObject.name.Colorize(Palette.GoldenAmber)} ({gameObject.GetInstanceID()}).");
                    Destroy(gameObject);
                    return;
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (gameObject.name != $"{typeof(T).Name} (Singleton {gameObject.GetInstanceID()})")
                gameObject.name = $"{typeof(T).Name} (Singleton {gameObject.GetInstanceID()})";
        }
#endif

        protected virtual void OnDestroy()
        {
            s_queueForDestroy = true;
            s_instance = null;
        }
    }
}

