using UnityEngine;

namespace NekoLib.Extensions
{
    public static class SerializeExtensions
    {
        public static string Serialize<T>(this T obj, bool prettyPrint = false)
        {
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        public static T Deserialize<T>(this string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}