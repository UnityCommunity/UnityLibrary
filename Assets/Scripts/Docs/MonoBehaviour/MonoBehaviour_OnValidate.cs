using UnityEngine;
using System.Collections;

// Example: Using OnValidate() to validate inspector fields in editor
// https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnValidate.html

namespace UnityLibrary
{
    public class MonoBehaviour_OnValidate : MonoBehaviour
    {
        // try setting this number larger than 100 in inspector
        public float number = 0;

        // this gets called only in editor, when inspector field is modified
        void OnValidate()
        {
            // you can print warnings also
            // if (number < 0 || number > 100) Debug.LogWarning("OnValidate: number value is invalid..");

            // clamp number to 0-100
            number = Mathf.Clamp(number, 0, 100);
        }
    }
}

