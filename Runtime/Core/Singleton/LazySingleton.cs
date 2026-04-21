using System;
using NekoLib.ColorPalette;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;

#if ODIN_INSPECTOR
using BaseBehaviour = Sirenix.OdinInspector.SerializedMonoBehaviour;
#else
using BaseBehaviour = UnityEngine.MonoBehaviour;
#endif

namespace NekoLib.Singleton
{
    /// <summary>
    /// Lazy singleton automatically creates a new singleton game object when a script uses it.
    /// Instance creation must be triggered from the main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class LazySingleton<T> : BaseBehaviour where T : MonoBehaviour
    {
        private static Lazy<T> s_lazyInstance = new(CreateSingleton);

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => s_lazyInstance = new Lazy<T>(CreateSingleton);
#endif

        public static T Instance => s_lazyInstance.Value;

        private static T CreateSingleton()
        {
            var ownerObject = new GameObject($"{typeof(T).Name} (Singleton)");
            var instance = ownerObject.AddComponent<T>();
            DontDestroyOnLoad(ownerObject);

            Log.Info($"Create a new singleton of type {typeof(T).Name.Colorize(Swatch.LA)}.");

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
