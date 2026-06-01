using TRnK.Extensions;
using TRnK.Logger;
using UnityEngine;

namespace TRnK.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("TRnK/Look At Camera")]
    public sealed class LookAtCamera : MonoBehaviour
    {
        private enum Mode
        {
            LookAt,                // Rotate to face the camera's position.
            LookAtInverted,        // Rotate to face away from the camera's position.
            CameraForward,         // Match the camera's forward direction (billboard-parallel).
            CameraForwardInverted  // Match the camera's backward direction.
        }

        [SerializeField] private Mode _mode;
        [SerializeField] private bool _useCustomCamera;
        [SerializeField] private Camera _cameraToLookAt;

        private Camera _cachedMainCamera;

        private void Start()
        {
            if (!_useCustomCamera)
            {
                _cachedMainCamera = Camera.main;
                if (_cachedMainCamera == null)
                {
                    Log.Warn("[LookAtCamera] No main camera found. Component will not function.");
                    enabled = false;
                }
            }
        }

        private void LateUpdate()
        {
            Camera currentCamera = _useCustomCamera ? _cameraToLookAt : _cachedMainCamera;

            if (currentCamera == null)
                return;

            switch (_mode)
            {
                case Mode.LookAt:
                    transform.LookAt(currentCamera.transform);
                    break;

                case Mode.LookAtInverted:
                    // Direction from camera to self = pointing away from camera.
                    var dirAwayFromCamera = currentCamera.transform.DirectionTo(transform);
                    transform.LookAt(transform.position + dirAwayFromCamera);
                    break;

                case Mode.CameraForward:
                    transform.forward = currentCamera.transform.forward;
                    break;

                case Mode.CameraForwardInverted:
                    transform.forward = -currentCamera.transform.forward;
                    break;
            }
        }
    }
}
