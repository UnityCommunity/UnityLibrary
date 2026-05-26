// easy way to get Toggle state as events (compared to the default OnChanged, which triggers on both..)

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityLibrary.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleEvents : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent OnChecked;
        public UnityEvent OnUnchecked;

        private Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(HandleToggleChanged);
        }

        private void OnDestroy()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            }
        }

        private void HandleToggleChanged(bool isOn)
        {
            if (isOn)
            {
                OnChecked?.Invoke();
            }
            else
            {
                OnUnchecked?.Invoke();
            }
        }
    }
}
