using UnityEngine;
using UnityEditor;

// draws camera frustum lines with Gizmo lines (when camera is not selected)
// Usage: add this script into Editor/ folder on your project
// WARNING: cam.transform.position does not work, DrawFrustum ignores the value, unity bug?
namespace UnityLibrary
{
    static class DrawCameraFrustumGizmos
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy)]// | GizmoType.Active)]
        static void DrawGizmoForMyScript(Camera cam, GizmoType gizmoType)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawFrustum(cam.transform.position, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
        }
    }
}