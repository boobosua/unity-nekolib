using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Utilities
{
    /// <summary>
    /// Singleton that will be destroyed when the scene is unloaded.
    /// Can only be used within a scene.
    /// Do not reference the instance in OnDestroy(), OnDisable() or OnApplicationQuit().
    /// </summary>
    /// <typeparam name="T"> The type of the singleton. </typeparam>
    [DisallowMultipleComponent]
    public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static bool s_isInitializing = false;

        public static T Instance
        {
            get
            {
                if (s_instance != null)
                {
                    // Debug.Log($"Returning existing instance of {typeof(T).Name.Colorize(Palette.Lavender)}.");
                    return s_instance;
                }

                s_isInitializing = true;

                var allInstances = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                if (allInstances.Length == 1)
                {
                    s_instance = allInstances[0];
                    // Debug.Log($"Using existing instance of {typeof(T).Name.Colorize(Palette.Lavender)}.");
                }
                else if (allInstances.Length > 1)
                {
                    // Debug.LogWarning($"Found multiple instances of {typeof(T).Name.Colorize(Palette.VibrantRed)}. Using the first one found.");
                    s_instance = allInstances[0];

                    for (int i = 1; i < allInstances.Length; i++)
                    {
                        Debug.LogWarning($"Destroying duplicate instance of {allInstances[i].name.Colorize(Palette.GoldenAmber)}.");
                        Destroy(allInstances[i].gameObject);
                    }
                }

                s_isInitializing = false;

                if (s_instance == null)
                {
                    Debug.LogError($"Instance {typeof(T).Name.Colorize(Palette.GoldenAmber)} is null.");
                }

                return s_instance;
            }
        }

        protected virtual void Awake()
        {
            if (s_instance == null && !s_isInitializing)
            {
                s_instance = this as T;
            }
            else if (s_instance != null && s_instance != this && !s_isInitializing)
            {
                Debug.LogWarning($"Destroying duplicate instance of {gameObject.name.Colorize(Palette.GoldenAmber)}.");
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (gameObject.name != $"{typeof(T).Name} (Singleton {gameObject.GetInstanceID()})")
                gameObject.name = $"{typeof(T).Name} (Singleton {gameObject.GetInstanceID()})";
        }
#endif
    }
}

