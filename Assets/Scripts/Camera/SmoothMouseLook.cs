// original source: http://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/
// Very simple smooth mouselook modifier for the MainCamera in Unity
// by Francis R. Griffiths-Keam - www.runningdimensions.com 
// Modified: Escape key for hide/show & lock/unlock mouse

using UnityEngine;
namespace UnityLibrary
{
    public class SmoothMouseLook : MonoBehaviour
    {
        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;

        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool lockCursor;
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector2 targetDirection;

        void Start()
        {
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.rotation.eulerAngles;
            Cursor.visible = !lockCursor;
        }

        void LateUpdate()
        {

            // pressing esc toggles between hide/show and lock/unlock cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                lockCursor = !lockCursor;
            }

            // Ensure the cursor is always locked when set
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;

            // Allow the script to clamp based on a desired target value.
            Quaternion targetOrientation = Quaternion.Euler(targetDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
            transform.rotation *= targetOrientation;
        }
    }
}