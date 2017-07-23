// OnPostprocessBuild() gets called after build has completed
// usage: copy to Editor/ folder in your project

using UnityEditor;
using UnityEditor.Build;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UnityLibrary
{
    public class PostProcessBuild : IPostprocessBuild
    {
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            Debug.Log("OnPostprocessBuild for target " + target + " at path " + path);

            // Run some process after successful build
            Process proc = new Process();
            proc.StartInfo.FileName = "C:/WINDOWS/system32/notepad.exe";
            proc.Start();
        }
    }
}
