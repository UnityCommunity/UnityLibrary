using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// source: https://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/#post-3101292
// added: lockCursor

namespace UnityLibrary
{
    public class SmoothMouseLookAveraged : MonoBehaviour
    {
        [Header("Info")]
        private List<float> _rotArrayX = new List<float>(); // TODO: could use fixed array, or queue
        private List<float> _rotArrayY = new List<float>();
        private float rotAverageX;
        private float rotAverageY;
        private float mouseDeltaX;
        private float mouseDeltaY;

        [Header("Settings")]
        public float _sensitivityX = 1.5f;
        public float _sensitivityY = 1.5f;
        [Tooltip("The more steps, the smoother it will be.")]
        public int _averageFromThisManySteps = 3;
        public bool lockCursor = false;

        [Header("References")]
        [Tooltip("Object to be rotated when mouse moves left/right.")]
        public Transform _playerRootT;
        [Tooltip("Object to be rotated when mouse moves up/down.")]
        public Transform _cameraT;

        //============================================
        // FUNCTIONS (UNITY)
        //============================================

        void Start()
        {
            Cursor.visible = !lockCursor;
        }

        void Update()
        {
            HandleCursorLock();
            MouseLookAveraged();
        }

        //============================================
        // FUNCTIONS (CUSTOM)
        //============================================

        void HandleCursorLock()
        {
            // pressing esc toggles between hide/show and lock/unlock cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                lockCursor = !lockCursor;
            }

            // Ensure the cursor is always locked when set
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
        }



        void MouseLookAveraged()
        {
            rotAverageX = 0f;
            rotAverageY = 0f;
            mouseDeltaX = 0f;
            mouseDeltaY = 0f;

            mouseDeltaX += Input.GetAxis("Mouse X") * _sensitivityX;
            mouseDeltaY += Input.GetAxis("Mouse Y") * _sensitivityY;

            // Add current rot to list, at end
            _rotArrayX.Add(mouseDeltaX);
            _rotArrayY.Add(mouseDeltaY);

            // Reached max number of steps? Remove oldest from list
            if (_rotArrayX.Count >= _averageFromThisManySteps)
                _rotArrayX.RemoveAt(0);

            if (_rotArrayY.Count >= _averageFromThisManySteps)
                _rotArrayY.RemoveAt(0);

            // Add all of these rotations together
            for (int i_counterX = 0; i_counterX < _rotArrayX.Count; i_counterX++)
                rotAverageX += _rotArrayX[i_counterX];

            for (int i_counterY = 0; i_counterY < _rotArrayY.Count; i_counterY++)
                rotAverageY += _rotArrayY[i_counterY];

            // Get average
            rotAverageX /= _rotArrayX.Count;
            rotAverageY /= _rotArrayY.Count;

            // Apply
            _playerRootT.Rotate(0f, rotAverageX, 0f, Space.World);
            _cameraT.Rotate(-rotAverageY, 0f, 0f, Space.Self);
        }
    }
}
