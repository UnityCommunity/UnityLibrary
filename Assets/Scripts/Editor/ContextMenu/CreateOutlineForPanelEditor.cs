// Custom Contect menu for UI Image component (in WorldSpace UI canvas)
// used for creating outline for panel (image) using LineRenderer

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace UnityLibrary
{
    public class CreateOutlineForPanelEditor : MonoBehaviour
    {
        [MenuItem("CONTEXT/Image/Create Outline For Panel")]
        static void DoubleMass(MenuCommand command)
        {
            // get reference
            Image comp = (Image)command.context;
            if (comp == null)
            {
                Debug.LogError("No Image component found..");
                return;
            }

            // TODO check that its worlspace canvas

            // get worldspace borders, FIXME get root canvas, instead of parent
            var canvasRect = comp.transform.parent.GetComponent<Canvas>().GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);

            var line = comp.transform.GetComponent<LineRenderer>();
            if (line == null)
            {
                Debug.LogError("Missing LineRenderer component");
                return;
            }

            if (line.useWorldSpace == true)
            {
                Debug.LogWarning("LineRenderer has worlspace enabled, disabling it");
                line.useWorldSpace = false;
            }

            // set line points
            line.positionCount = corners.Length + 1;
            line.SetPositions(corners);
            // connect last and first
            line.SetPosition(line.positionCount - 1, corners[0]);

            // convert worldspace to localspace
            for (int i = 0, len = line.positionCount; i < len; i++)
            {
                var worldPos = line.GetPosition(i);
                var localPos = canvasRect.transform.InverseTransformPoint(worldPos);
                line.SetPosition(i, localPos);
            }

        }
    }
}
