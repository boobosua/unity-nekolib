using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NekoLib.Extensions;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
        /// <summary>
        /// Returns 'true' if we touched or hovering on Unity UI element.
        /// </summary>
        public static bool IsPointerOverUI(LayerMask layer)
        {
            var eventSystemRaycastResults = GetEventSystemRaycastResults();
            if (eventSystemRaycastResults.IsNullOrEmpty())
                return false;

            for (int i = 0; i < eventSystemRaycastResults.Count; i++)
            {
                RaycastResult curRaycastResult = eventSystemRaycastResults[i];
                if (curRaycastResult.gameObject.IsInLayer(layer))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all event system raycast results of current mouse or touch position.
        /// </summary>
        private static List<RaycastResult> GetEventSystemRaycastResults()
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("EventSystem is null. Returning empty raycast results.");
                return new List<RaycastResult>();
            }

            PointerEventData eventData = new(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults;
        }
    }
}
