using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

// copies empty StreamingAssets/ folders into build, as they are not automatically included

namespace UnityLibrary
{
    public class PostBuildCopyEmptyFolders : MonoBehaviour
    {
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            // only for windows
            if (target != BuildTarget.StandaloneWindows) return;
        
            Debug.Log("<color=#00FF00>### POSTBUILD : COPY EMPTY STREAMINGASSETS-FOLDERS ###</color>");
            Debug.Log("Build done: " + pathToBuiltProject);

            // get output root
            var root = Path.GetDirectoryName(pathToBuiltProject);
            var appName = Path.GetFileNameWithoutExtension(pathToBuiltProject);

            // copy empty streaming asset folders to build
            var sourcePath = Application.streamingAssetsPath;
            var targetPath = Path.Combine(root, appName + "_Data", "StreamingAssets");
            //Debug.Log("sourcePath= "+ sourcePath);
            //Debug.Log("targetPath= " + targetPath);
            CopyFolderStructure(sourcePath, targetPath);
        }

        // recursive folder copier
        static public void CopyFolderStructure(string sourceFolder, string destFolder)
        {
            if (Directory.Exists(destFolder))
            {

            }
            else
            {
                Directory.CreateDirectory(destFolder);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolderStructure(folder, dest);
            }
        }
    }
}
