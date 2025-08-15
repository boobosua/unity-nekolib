using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib
{
    [DisallowMultipleComponent]
    public class LookAtCamera : MonoBehaviour
    {
        private enum Mode
        {
            LookAt, // Look at camera center point.
            LookAtInverted, // Look at camera center point in an opposite direction.
            CameraForward, // Look straight at camera in X position only.
            CameraForwardInverted // Look straight at camera in X position only but in an opposite direction.
        }

        [SerializeField] private Mode _mode;
        [SerializeField] private bool _useCustomCamera;
        [SerializeField] private Camera _cameraToLookAt;

        private void LateUpdate()
        {
            var currentCamera = Camera.main;

            if (_useCustomCamera && _cameraToLookAt != null)
            {
                currentCamera = _cameraToLookAt;
            }

            if (currentCamera == null)
            {
                Debug.LogWarning("[LookAtCamera] No camera found. Component will not function.");
                return;
            }

            switch (_mode)
            {
                case Mode.LookAt:
                    transform.LookAt(currentCamera.transform);
                    break;

                case Mode.LookAtInverted:
                    var dirFromCamera = transform.DirectionTo(currentCamera.transform);
                    transform.LookAt(transform.position + dirFromCamera);
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
