#if UNITY_EDITOR
using System;
using UnityEngine;

namespace NekoLib
{
    public enum DocCategory { Core, Components, Extensions, Collections, Services, Utilities, EditorTools, NekoSignal, NekoFlow, NekoSerializer }

    [Serializable]
    public sealed class NekoLibDocEntry
    {
        public string Title;
        public string Namespace;
        [TextArea(2, 4)] public string Summary;
        [TextArea(3, 12)] public string Description;
        [TextArea(4, 30)] public string Code;
        public string[] Tags;
        public DocCategory Category;
    }
}
#endif
