// sets UI element selected
// usage: attach to gameobject with UI selectable component (inputfield, button, dropdown. toggle..)

using UnityEngine;
using UnityEngine.UI;

namespace UnityLibrary
{
    public class SetSelected : MonoBehaviour
    {
        void Start()
        {
            var element = GetComponent<Selectable>();

            if (element != null && element.interactable == true)
            {
                element.Select();
            } else
            {
                Debug.LogWarning("Nothing to set selected..", gameObject);
            }
        }
    }
}
