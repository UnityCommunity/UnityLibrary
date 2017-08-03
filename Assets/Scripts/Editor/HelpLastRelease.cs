/*
 * Version: 2.4
 * Fork from the original script: https://pastebin.com/LzEHNB6U
 * Purpose: Download new versions of Unity (alpha, beta,
 *          patches and releases) straight from the Editor.
 *          Shortcut in the editor for Statistics, Experimental
 *          Features and the Unity Roadmap.
 *          
 * Usage:   Menu, Links, Last Releases.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace UnityLibrary
{
    public class HelpLastRelease : EditorWindow
    {
        const string statsUrl = @"http://hwstats.unity3d.com/index.html";
        const string experimenalUrl = @"http://unity3d.com/experimental";
        const string roadmapUrl = @"http://unity3d.com/unity/roadmap";
        const string archiveUrl = @"http://unity3d.com/get-unity/download/archive";
        const string betaArchiveUrl = @"http://unity3d.com/unity/beta/archive";
        const string releaseUrl = @"http://beta.unity3d.com/download/{0}/download.html";
        const string assistantUrl = @"http://beta.unity3d.com/download/{0}/UnityDownloadAssistant-{1}.{2}";
        const string serverUrl = @"http://symbolserver.unity3d.com/";
        const string historyUrl = serverUrl + @"000Admin/history.txt";
        const string finalRN = @"http://unity3d.com/unity/whats-new/unity-";
        const string betaRN = @"http://unity3d.com/unity/beta/unity";
        const string patchRN = @"http://unity3d.com/unity/qa/patch-releases";

        const string baseName = "UnityYAMLMerge.ex";
        const string compressedName = baseName + "_";
        const string extractedName = baseName + "e";
        static string tempDir;
        static WWW wwwHistory, wwwList, wwwMerger, wwwAssistant;
        static readonly string zipName = Application.platform == RuntimePlatform.WindowsEditor ? "7z" : "7za";

        static SortedList<string, string> fullList;
        static SortedList<string, string> sortedList;
        static SortedList<string, string> currentList;
        static int selected;
        static bool assistant;
        static HelpLastRelease window;
        const string wndTitle = "Unity Builds";
        [SerializeField]
        string search = "";
        static GUIStyle style;
        const string prefsCount = "HelpLastRelease.count";
        const float minWidth = 162f;
        [SerializeField]
        private Vector2 scroll;
        static Dictionary<string, Color32> colors = new Dictionary<string, Color32>() {
        { "5.0.", new Color32(236, 239, 241 ,255) },
        { "5.1.", new Color32(207, 216, 220, 255) },
        { "5.2.", new Color32(176, 190, 197, 255) },
        { "5.3.", new Color32(144, 164, 174, 255) },
        { "5.4.", new Color32(120, 144, 156, 255) },
        { "5.5.", new Color32(96, 125, 139, 255) },
        { "5.6.", new Color32(84, 110, 122, 255) },
        { "2017.1.", new Color32(69, 90, 100, 255) },
        { "2017.2.", new Color32(55, 71, 79, 255) },
        { "2017.3.", new Color32(38, 50, 56, 255) }
    };

        static Dictionary<string, Color32> textColors = new Dictionary<string, Color32>() {
        { "5.0.", new Color32(0, 0, 0, 255) },
        { "5.1.", new Color32(0, 0, 0, 255) },
        { "5.2.", new Color32(0, 0, 0, 255) },
        { "5.3.", new Color32(0, 0, 0, 255) },
        { "5.4.", new Color32(255, 255, 255, 255) },
        { "5.5.", new Color32(255, 255, 255, 255) },
        { "5.6.", new Color32(255, 255, 255, 255) },
        { "2017.1.", new Color32(255, 255, 255, 255) },
        { "2017.2.", new Color32(255, 255, 255, 255) },
        { "2017.3.", new Color32(255, 255, 255, 255) }
    };

        [MenuItem("Help/Links/Statistics")]
        static void OpenStatistics()
        {
            Application.OpenURL(statsUrl);
        }

        [MenuItem("Help/Links/Experimental")]
        static void OpenExperimental()
        {
            Application.OpenURL(experimenalUrl);
        }

        [MenuItem("Help/Links/Roadmap")]
        static void OpenRoadmap()
        {
            Application.OpenURL(roadmapUrl);
        }

        [MenuItem("Help/Links/Release Archive")]
        static void OpenArchive()
        {
            Application.OpenURL(archiveUrl);
        }

        [MenuItem("Help/Links/Patch Archive")]
        static void OpenPatchArchive()
        {
            Application.OpenURL(patchRN);
        }

        [MenuItem("Help/Links/Beta Archive")]
        static void OpenBetaArchive()
        {
            Application.OpenURL(betaArchiveUrl);
        }

        [MenuItem("Help/Links/Last Releases")]
        static void Init()
        {
            window = GetWindow<HelpLastRelease>(wndTitle);
        }

        void OnGUI()
        {
            if (fullList != null)
            {
                ListGUI();
            }
            else
                WaitGUI();
        }

        public void ListGUI()
        {
            style = new GUIStyle(EditorStyles.miniButton);
            style.alignment = TextAnchor.MiddleLeft;
            //GUILayout.BeginVertical();
            DoToolbar();
            scroll = EditorGUILayout.BeginScrollView(scroll, false, false);
            if (currentList == null)
                currentList = fullList;

            for (int i = currentList.Count - 1; i >= 0; i--)
                DoItemGUI(i, currentList.Keys[i], currentList.Values[i]);

            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            DoDownloadProgressBar(wwwAssistant, "Downloading Assistant");
            DoDownloadProgressBar(wwwHistory, "Downloading History");
            DoDownloadProgressBar(wwwList, "Downloading List");
            DoDownloadProgressBar(wwwMerger, "Downloading Merger");
            //GUILayout.EndVertical();
        }

        private void Update()
        {
            Repaint();
        }

        void DoDownloadProgressBar(WWW www, string text)
        {
            if (www != null && !www.isDone && string.IsNullOrEmpty(www.error))
            {
                text = string.Format("{0} ({1})", text, EditorUtility.FormatBytes(www.bytesDownloaded));
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), www.progress, string.IsNullOrEmpty(www.error) ? text : www.error);
            }
        }

        void DoItemGUI(int index, string key, string value)
        {
            var rect = EditorGUILayout.BeginHorizontal((index & 1) == 1 ? "ObjectPickerResultsOdd" : "ObjectPickerResultsEven");
            var color = GetGUIColor(index);
            var colorText = GetGUITextColor(index);
            var s = new GUIStyle();
            s.normal.textColor = colorText;

            EditorGUI.DrawRect(rect, color);
            GUILayout.Label("Unity " + value, s);
            GUILayout.FlexibleSpace();

            var notesURL = GetReleaseNotesURL(index);
            if (GUILayout.Button("Open", "minibuttonleft"))
            {
                DownloadList(index);
            }
            GUI.enabled = !string.IsNullOrEmpty(notesURL);
            if (GUILayout.Button("Release Notes", "minibuttonmid"))
            {
                OpenReleaseNotes(index);
            }
            GUI.enabled = true;
            if (GUILayout.Button("Download Assistant", "minibuttonright"))
            {
                DownloadList(index, true);
            }
            EditorGUILayout.EndHorizontal();
        }

        static Color32 GetGUIColor(int i)
        {
            foreach (var k in colors.Keys)
            {
                if (currentList.Values[i].Contains(k))
                {
                    return colors[k];
                }
            }
            return new Color32(255, 255, 255, 255);
        }

        static Color32 GetGUITextColor(int i)
        {
            foreach (var k in textColors.Keys)
            {
                if (currentList.Values[i].Contains(k))
                {
                    return textColors[k];
                }
            }
            return new Color32(0, 0, 0, 255);
        }

        void OnEnable()
        {
            tempDir = Application.dataPath + "/../Temp/LastRelease";
            DownloadHistory();
        }

        static void CheckNewVersion()
        {
            int count = EditorPrefs.GetInt(prefsCount, 0);
            if (count > 0 && fullList.Count > count)
            {
                EditorApplication.Beep();
                Debug.LogFormat("New version: <color=yellow>{0}</color>", fullList.Values[fullList.Count - 1]);
            }
            EditorPrefs.SetInt(prefsCount, fullList.Count);
        }

        static string GetReleaseNotesURL(int num)
        {
            string url = "", version = "";
            if (currentList.Values[num].Contains("a"))
                return string.Empty;
            if (currentList.Values[num].Contains("p"))
            {
                version = currentList.Values[num].Split(' ')[0];
                url = patchRN + version;
            }
            if (currentList.Values[num].Contains("f"))
            {
                version = currentList.Values[num].Split('f')[0];
                url = finalRN + version;
            }
            if (currentList.Values[num].Contains("b"))
            {
                version = currentList.Values[num].Split(' ')[0];
                url = betaRN + version;
            }

            return url;
        }

        static void OpenReleaseNotes(int num)
        {
            var url = GetReleaseNotesURL(num);

            if (!string.IsNullOrEmpty(url))
                Application.OpenURL(url);
        }

        static void FillMenu(WWW history)
        {
            fullList = new SortedList<string, string>();
            string build;
            string[] parts, releases = history.text.Split('\n');
            for (int i = 0; i < releases.Length; i++)
            {
                parts = releases[i].Split(',');
                DateTime dt;
                if (DateTime.TryParse(string.Format("{0} {1}", parts[3], parts[4]), out dt))
                {
                    build = string.Format("{0} ({1})", parts[6].Trim('\"'), dt.ToString("dd-MM-yyyy"));
                    fullList.Add(parts[0], build);
                }
                //Debug.LogWarningFormat("releases[{0}]={1}\nparts={2}", i, releases[i], parts.ToStringRecursive());
            }
            if (window == null)
            {
                HelpLastRelease[] w = Resources.FindObjectsOfTypeAll<HelpLastRelease>();
                if (w != null && w.Length > 0)
                    window = w[0];
            }
            if (window != null)
                window.Repaint();
        }

        static void SearchVersion()
        {
            string path = Path.Combine(tempDir, extractedName);
            if (File.Exists(path))
            {
                string[] lines;
                lines = File.ReadAllLines(path, Encoding.Unicode);
                FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(path));
                string version = currentList.Values[selected].Split(' ')[0] + "_";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(version))
                    {
                        int pos = lines[i].IndexOf(version);
                        string revision = lines[i].Substring(pos + version.Length, 12);
                        if (!assistant)
                        {
                            Application.OpenURL(string.Format(releaseUrl, revision));
                        }
                        else
                        {
                            DownloadAssistant(revision);
                        }
                        break;
                    }
                }
            }
        }

        static void DownloadAssistant(string revision)
        {
            string version = currentList.Values[selected].Split(' ')[0];
            string ext = Application.platform == RuntimePlatform.WindowsEditor ? "exe" : "dmg";
            string url = string.Format(assistantUrl, revision, version, ext);
            wwwAssistant = new WWW(url);
            EditorApplication.update += WaitAssistant;
        }

        static void DownloadHistory()
        {
            wwwHistory = new WWW(historyUrl);
            EditorApplication.update += WaitHistory;
        }

        static void DownloadList(int historyNum, bool assist = false)
        {
            selected = historyNum;
            assistant = assist;
            string listUrl = string.Format("{0}000Admin/{1}", serverUrl, currentList.Keys[historyNum]);
            wwwList = new WWW(listUrl);
            EditorApplication.update += WaitList;
        }

        static void WaitList()
        {
            Wait(wwwList, WaitList, ParseList);
        }

        static void WaitHistory()
        {
            Wait(wwwHistory, WaitHistory, FillMenu, CheckNewVersion);
        }

        static void WaitAssistant()
        {
            Wait(wwwAssistant, WaitAssistant, SaveAssistant);
        }

        static void SaveAssistant(WWW assistant)
        {
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            string name = Path.GetFileName(assistant.url);
            string path = Path.Combine(tempDir, name);
            File.WriteAllBytes(path, assistant.bytes);
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                Application.OpenURL(path);
            }
            else
            {
                StartAssistant(path);
            }
        }

        static void StartAssistant(string path)
        {
            string cmd = "hdiutil";
            string arg = string.Format("mount '{0}'", path);
            try
            {
                using (Process assist = new Process())
                {
                    assist.StartInfo.FileName = cmd;
                    assist.StartInfo.Arguments = arg;
                    assist.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
                    assist.StartInfo.CreateNoWindow = true;
                    assist.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    assist.Start();
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0} {1}\n{2}", cmd, arg, e.Message);
            }
        }

        static void Wait(WWW www, EditorApplication.CallbackFunction caller, Action<WWW> action, Action done = null)
        {
            if (www != null && www.isDone)
            {
                EditorApplication.update -= caller;
                if (string.IsNullOrEmpty(www.error) && www.bytesDownloaded > 0)
                {
                    //Debug.LogFormat("{0} kB: {1}", www.size/1024, www.url);
                    if (action != null)
                        action(www);
                    if (done != null)
                        done();
                }
                else
                    Debug.LogWarningFormat("{0} {1}", www.url, www.error);
                www = null;
            }
            else
            {
                if (www == null)
                    EditorApplication.update -= caller;
            }
        }

        static void ParseList(WWW list)
        {
            string[] files = list.text.Split('\n');
            string[] parts;
            for (int i = 0; i < files.Length; i++)
            {
                parts = files[i].Split(',');
                if (parts[0].Contains(extractedName))
                {
                    string mergerUrl = string.Format("{0}{1}/{2}", serverUrl, parts[0].Trim('\"').Replace('\\', '/'), compressedName);
                    DownloadMerger(mergerUrl);
                    break;
                }
            }
        }

        static void DownloadMerger(string mergerUrl)
        {
            wwwMerger = new WWW(mergerUrl);
            EditorApplication.update += WaitMerger;
        }

        static void WaitMerger()
        {
            Wait(wwwMerger, WaitMerger, SaveMerger);
        }

        static void SaveMerger(WWW merger)
        {
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            string path = Path.Combine(tempDir, compressedName);
            //Debug.LogFormat("path: {0}", path);
            File.WriteAllBytes(path, merger.bytes);
            ExtractMerger(path);
        }

        static void ExtractMerger(string path)
        {
            string zipPath = string.Format("{0}/Tools/{1}", EditorApplication.applicationContentsPath, zipName);
            string arg = string.Format("e -y \"{0}\"", path);
            try
            {
                using (Process unzip = new Process())
                {
                    unzip.StartInfo.FileName = zipPath;
                    unzip.StartInfo.Arguments = arg;
                    unzip.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
                    unzip.StartInfo.CreateNoWindow = true;
                    unzip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    unzip.Start();
                    unzip.WaitForExit();
                    SearchVersion();
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0} {1}\n{2}", zipPath, arg, e.Message);
            }
        }

        void DoToolbar()
        {
            EditorGUILayout.BeginHorizontal("toolbar");
            GUILayout.FlexibleSpace();
            SearchGUI();
            EditorGUILayout.EndHorizontal();
        }

        void SearchGUI()
        {
            string s = string.Empty;

            try
            {
                var methods = typeof(EditorGUILayout).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

                for (var i = 0; i < methods.Length; i++)
                    if (methods[i].Name == "ToolbarSearchField" && methods[i].GetParameters().Length <= 2)
                    {
                        s = (string)methods[i].Invoke(null, new object[] { search, null });
                        break;
                    }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                s = EditorGUILayout.TextField(search, GUILayout.MaxWidth(minWidth + 56f));
            }

            if (s != search)
            {
                search = s;
                if (!string.IsNullOrEmpty(search))
                {
                    sortedList = new SortedList<string, string>();
                    for (int i = fullList.Count - 1; i >= 0; i--)
                    {
                        if (fullList.Values[i].Contains(search))
                        {
                            sortedList.Add(fullList.Keys[i], fullList.Values[i]);
                        }
                    }
                    currentList = sortedList;
                }
                else
                    currentList = fullList;
            }
        }

        void WaitGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Wait...");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}