// Renames child gameobjects in hierarchy (by replacting strings)
// open wizard from GameObject/MassRenameChildren menu item

using UnityEditor;
using UnityEngine;

namespace UnityLibrary
{
    public class MassRenameChildren : ScriptableWizard
    {
        public string findString = "";
        public string replaceWith = "";

        [MenuItem("GameObject/Mass Rename Children")]
        static void CreateWizard()
        {
            DisplayWizard<MassRenameChildren>("MassRenamer", "Apply");
        }

        // user clicked create button
        void OnWizardCreate()
        {
            if (Selection.activeTransform == null || findString == "")
            {
                Debug.Log(name + " Select Root Transform and set FindString first..");
                return;
            }

            // get all children for the selection, NOTE: includeInactive is true, so disabled objects will get selected also
            Transform[] allChildren = Selection.activeTransform.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (Transform child in allChildren)
            {
                // skip self (selection root)
                if (child != Selection.activeTransform)
                {
                    var newName = child.name.Replace(findString, replaceWith);
                    Debug.LogFormat("Before: {0} | After: {1}", child.name, newName);
                    child.name = newName;
                }
            }
        }
        
    }
}
