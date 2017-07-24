// sets input field selected (so that can start typing on it)
// usage: attach to UI InputField gameobject

using UnityEngine;
using UnityEngine.UI;

namespace UnityLibrary
{
    [RequireComponent(typeof(InputField))]
    public class SetInputFieldSelected : MonoBehaviour
    {
        void Start()
        {
            var inputField = GetComponent<InputField>();
            inputField.Select();
        }
    }
}
