// Custom Contect menu for VideoPlayer component
// used for scaling quad mesh transform localscale.y to match videoplayer aspect ratio

using UnityEngine;
using UnityEditor;
using UnityEngine.Video;

namespace UnityLibrary
{
    public class GetVideoAspectRatioEditor : MonoBehaviour
    {
        [MenuItem("CONTEXT/VideoPlayer/Get Aspect Ratio for Mesh")]
        static void DoubleMass(MenuCommand command)
        {
            // get aspect ratio
            VideoPlayer v = (VideoPlayer)command.context;
            if (v.clip == null)
            {
                Debug.LogError("No videoclip assigned..");
                return;
            }
            float aspectRatioY = v.height / (float)v.width;

            // record undo
            Undo.RecordObject(v.transform, "Set scale");

            // scale mesh
            Vector3 scale = v.transform.localScale;
            // fix only height
            scale.y *= aspectRatioY;
            v.transform.localScale = scale;
        }
    }
}
