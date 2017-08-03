using UnityEngine;
using UnityEngine.UI;

// Usage: attach this to UI Text component
// it displays current version number from Ios/Android player settings with Application.version
namespace UnityLibrary
{
    public class GetVersion : MonoBehaviour
    {
        void Awake()
        {
            var t = GetComponent<Text>();
            if (t != null)
            {
                t.text = "v" + Application.version;
            }
        }
    }
}