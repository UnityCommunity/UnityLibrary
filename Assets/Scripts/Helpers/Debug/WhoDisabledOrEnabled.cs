// shows who disabled/enabled this gameobject from another script 
// usage: attach to gameobject that you want to track (see console stack trace)

using UnityEngine;

namespace UnityLibrary
{
    public class WhoDisabledOrEnabled : MonoBehaviour
    {
        private void OnDisable()
        {
            Debug.LogError("OnDisable: " + gameObject.name, gameObject);
        }

        private void OnEnable()
        {
            Debug.LogError("OnEnable: " + gameObject.name, gameObject);
        }
    }
}
