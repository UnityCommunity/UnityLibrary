// AndroidStoreCaptureTool.cs
// Put this file anywhere under an "Editor" folder.
// Usage:
// 1) Enter Play Mode.
// 2) Open: Tools/Android Store Capture
// 3) Pick output folder and click "Capture All Presets"

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Tools
{
    public class AndroidStoreCaptureTool : EditorWindow
    {
        [Serializable]
        private class Preset
        {
            public string name;          // base file name (without _WxH)
            public int width;
            public int height;
            public CropMode cropMode;

            public Preset(string name, int w, int h, CropMode cropMode)
            {
                this.name = name;
                width = w;
                height = h;
                this.cropMode = cropMode;
            }
        }

        private enum CropMode
        {
            Stretch,     // no crop, just scale to target (may distort)
            CropToFit    // center-crop to target aspect, then scale (no distortion)
        }

        private string _outputFolder = "StoreCaptures";
        private int _phoneCount = 2; // Play Console: 2-8 phone screenshots

        // Jobs
        private class CaptureJob
        {
            public Preset preset;
            public string filename;
        }

        private readonly Queue<CaptureJob> _queue = new Queue<CaptureJob>();
        private bool _isRunning;

        // Hidden helper MonoBehaviour that runs coroutines in Play Mode
        private CaptureHelper _helper;

        // Presets based on Play Console rules in your message.
        // Phone/tablet sizes are common choices within allowed ranges.
        private List<Preset> BuildPresets()
        {
            var list = new List<Preset>();

            // App icon and feature graphic
            list.Add(new Preset("appicon", 512, 512, CropMode.CropToFit));
            list.Add(new Preset("featuregraphic", 1024, 500, CropMode.CropToFit));

            // Phone screenshots (2-8). 9:16 or 16:9. Each side 320..3840.
            // We capture portrait by default; toggle to landscape if you want.
            for (int i = 1; i <= Mathf.Clamp(_phoneCount, 2, 8); i++)
                list.Add(new Preset("phone_" + i.ToString("00"), 1080, 1920, CropMode.CropToFit));

            // 7-inch tablet screenshots (allowed: 320..3840 each side)
            list.Add(new Preset("tablet7_01", 1920, 1200, CropMode.CropToFit)); // landscape 16:10
            list.Add(new Preset("tablet7_02", 1200, 1920, CropMode.CropToFit)); // portrait 10:16

            // 10-inch tablet screenshots (each side 1080..7680)
            list.Add(new Preset("tablet10_01", 2560, 1600, CropMode.CropToFit)); // landscape 16:10
            list.Add(new Preset("tablet10_02", 1600, 2560, CropMode.CropToFit)); // portrait 10:16

            return list;
        }

        [MenuItem("Tools/Android Store Capture")]
        public static void Open()
        {
            var w = GetWindow<AndroidStoreCaptureTool>("Android Store Capture");
            w.minSize = new Vector2(420, 340);
            w.Show();
        }

        private void OnDisable()
        {
            StopRunner();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Capture from Game View (Play Mode)", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                _outputFolder = EditorGUILayout.TextField("Folder", _outputFolder);
                if (GUILayout.Button("Browse", GUILayout.Width(80)))
                {
                    string picked = EditorUtility.OpenFolderPanel("Pick output folder", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(picked))
                    {
                        // Make it project-relative when possible
                        string proj = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                        string full = Path.GetFullPath(picked);
                        if (full.StartsWith(proj, StringComparison.OrdinalIgnoreCase))
                        {
                            _outputFolder = full.Substring(proj.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        }
                        else
                        {
                            _outputFolder = full;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                _phoneCount = EditorGUILayout.IntSlider("Phone screenshots (2-8)", _phoneCount, 2, 8);
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter Play Mode first. This tool captures the rendered Game View.", MessageType.Warning);
                }

                GUI.enabled = EditorApplication.isPlaying && !_isRunning;
                if (GUILayout.Button("Capture All Presets"))
                {
                    EnqueueAll();
                    StartRunner();
                }

                if (GUILayout.Button("Capture Only Icon + Feature Graphic"))
                {
                    EnqueueIconAndFeatureOnly();
                    StartRunner();
                }
                GUI.enabled = true;

                GUI.enabled = _isRunning;
                if (GUILayout.Button("Stop"))
                {
                    StopRunner();
                }
                GUI.enabled = true;

                if (_isRunning)
                {
                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField("Running...", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Remaining", _queue.Count.ToString());
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("- Files are named like: appicon_512x512.png, featuregraphic_1024x500.png");
                EditorGUILayout.LabelField("- Phone screenshots are named like: phone_01_1080x1920.png");
                EditorGUILayout.LabelField("- Captures center-crop to match target aspect (no stretching).");
            }
        }

        private void EnqueueAll()
        {
            _queue.Clear();

            string folder = ResolveOutputFolder();
            Directory.CreateDirectory(folder);

            foreach (var p in BuildPresets())
            {
                string fn = $"{p.name}_{p.width}x{p.height}.png";
                _queue.Enqueue(new CaptureJob { preset = p, filename = Path.Combine(folder, fn) });
            }
        }

        private void EnqueueIconAndFeatureOnly()
        {
            _queue.Clear();

            string folder = ResolveOutputFolder();
            Directory.CreateDirectory(folder);

            var icon = new Preset("appicon", 512, 512, CropMode.CropToFit);
            var feature = new Preset("featuregraphic", 1024, 500, CropMode.CropToFit);

            _queue.Enqueue(new CaptureJob { preset = icon, filename = Path.Combine(folder, $"appicon_512x512.png") });
            _queue.Enqueue(new CaptureJob { preset = feature, filename = Path.Combine(folder, $"featuregraphic_1024x500.png") });
        }

        private string ResolveOutputFolder()
        {
            // If user gave absolute path, use it. Otherwise, place under project root.
            if (Path.IsPathRooted(_outputFolder))
                return _outputFolder;

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, _outputFolder);
        }

        private CaptureHelper EnsureHelper()
        {
            if (_helper != null) return _helper;

            var go = new GameObject("[AndroidStoreCaptureHelper]")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _helper = go.AddComponent<CaptureHelper>();
            return _helper;
        }

        private void StartRunner()
        {
            if (_isRunning) return;
            if (!EditorApplication.isPlaying) return;

            _isRunning = true;

            GetMainGameView();

            var helper = EnsureHelper();
            helper.StartCoroutine(RunCaptures());
        }

        private void StopRunner()
        {
            _isRunning = false;
            _queue.Clear();

            if (_helper != null)
            {
                _helper.StopAllCoroutines();
                DestroyImmediate(_helper.gameObject);
                _helper = null;
            }
        }

        private IEnumerator RunCaptures()
        {
            while (_queue.Count > 0)
            {
                if (!EditorApplication.isPlaying)
                {
                    StopRunner();
                    yield break;
                }

                var job = _queue.Dequeue();

                SetGameViewSize(job.preset.width, job.preset.height);

                // Wait for the GameView to resize and re-render
                for (int i = 0; i < 6; i++)
                    yield return null;

                // Wait for end of frame — this is required for ScreenCapture to work
                yield return new WaitForEndOfFrame();

                try
                {
                    ProcessCaptureJob(job);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Capture failed: " + ex);
                }

                Repaint();
            }

            _isRunning = false;
            AssetDatabase.Refresh();
            Debug.Log("Android Store Capture: All captures finished.");
            Repaint();

            if (_helper != null)
            {
                DestroyImmediate(_helper.gameObject);
                _helper = null;
            }
        }

        private void ProcessCaptureJob(CaptureJob job)
        {
            int targetW = job.preset.width;
            int targetH = job.preset.height;

            Texture2D src = ScreenCapture.CaptureScreenshotAsTexture();
            if (src == null)
            {
                Debug.LogError($"CaptureScreenshotAsTexture returned null for {job.filename}. Skipping.");
                return;
            }

            Texture2D processed;
            if (job.preset.cropMode == CropMode.CropToFit)
                processed = CropToAspectThenScale(src, targetW, targetH);
            else
                processed = ScaleTexture(src, targetW, targetH);

            byte[] png = processed.EncodeToPNG();
            File.WriteAllBytes(job.filename, png);

            DestroyImmediate(src);
            if (processed != src)
                DestroyImmediate(processed);

            Debug.Log("Saved: " + job.filename);
        }

        private static Texture2D CropToAspectThenScale(Texture2D src, int targetW, int targetH)
        {
            float srcAspect = (float)src.width / src.height;
            float dstAspect = (float)targetW / targetH;

            int cropW = src.width;
            int cropH = src.height;

            if (srcAspect > dstAspect)
            {
                // too wide -> crop width
                cropW = Mathf.RoundToInt(src.height * dstAspect);
                cropH = src.height;
            }
            else
            {
                // too tall -> crop height
                cropW = src.width;
                cropH = Mathf.RoundToInt(src.width / dstAspect);
            }

            // Crop from top-left: x starts at 0, y starts from top
            int x0 = 0;
            int y0 = src.height - cropH;

            Color[] pixels = src.GetPixels(x0, y0, cropW, cropH);
            Texture2D cropped = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            cropped.SetPixels(pixels);
            cropped.Apply(false, false);

            Texture2D scaled = ScaleTexture(cropped, targetW, targetH);
            DestroyImmediate(cropped);
            return scaled;
        }

        private static Texture2D ScaleTexture(Texture2D src, int targetW, int targetH)
        {
            Texture2D dst = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false);

            for (int y = 0; y < targetH; y++)
            {
                float v = (targetH == 1) ? 0f : (float)y / (targetH - 1);
                for (int x = 0; x < targetW; x++)
                {
                    float u = (targetW == 1) ? 0f : (float)x / (targetW - 1);
                    Color c = SampleBilinear(src, u, v);
                    dst.SetPixel(x, y, c);
                }
            }

            dst.Apply(false, false);
            return dst;
        }

        private static Color SampleBilinear(Texture2D tex, float u, float v)
        {
            float x = u * (tex.width - 1);
            float y = v * (tex.height - 1);

            int x0 = Mathf.Clamp((int)Mathf.Floor(x), 0, tex.width - 1);
            int y0 = Mathf.Clamp((int)Mathf.Floor(y), 0, tex.height - 1);
            int x1 = Mathf.Clamp(x0 + 1, 0, tex.width - 1);
            int y1 = Mathf.Clamp(y0 + 1, 0, tex.height - 1);

            float tx = x - x0;
            float ty = y - y0;

            Color c00 = tex.GetPixel(x0, y0);
            Color c10 = tex.GetPixel(x1, y0);
            Color c01 = tex.GetPixel(x0, y1);
            Color c11 = tex.GetPixel(x1, y1);

            Color a = Color.Lerp(c00, c10, tx);
            Color b = Color.Lerp(c01, c11, tx);
            return Color.Lerp(a, b, ty);
        }

        // ---------------------------
        // GameView sizing (internal)
        // ---------------------------

        private static EditorWindow GetMainGameView()
        {
            Type t = Type.GetType("UnityEditor.GameView,UnityEditor");
            if (t == null) return null;

            // Try "GetMainGameView" first (older Unity versions)
            MethodInfo getMain = t.GetMethod("GetMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            if (getMain != null)
            {
                var result = getMain.Invoke(null, null) as EditorWindow;
                if (result != null) return result;
            }

            // Fallback: try "GetMainGameViewRenderRect" or just find an open GameView window
            var gameView = GetWindow(t, false, null, false);
            return gameView;
        }

        private static void SetGameViewSize(int width, int height)
        {
            // Creates/uses a fixed resolution entry in the current platform group, then selects it.
            // Unity does not expose this publicly; reflection is used.

            Type sizesType = Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
            Type sizeType = Type.GetType("UnityEditor.GameViewSize,UnityEditor");
            Type groupType = Type.GetType("UnityEditor.GameViewSizeGroupType,UnityEditor");

            if (sizesType == null || sizeType == null || groupType == null)
                return;

            var instanceProp = sizesType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProp == null) return;
            object sizesInstance = instanceProp.GetValue(null, null);
            if (sizesInstance == null) return;

            MethodInfo getGroup = sizesType.GetMethod("GetGroup");
            if (getGroup == null) return;
            object group = getGroup.Invoke(sizesInstance, new object[] { (int)Enum.Parse(groupType, "Standalone") });
            if (group == null) return;

            // Find existing
            MethodInfo getBuiltinCount = group.GetType().GetMethod("GetBuiltinCount");
            MethodInfo getCustomCount = group.GetType().GetMethod("GetCustomCount");
            MethodInfo getGameViewSize = group.GetType().GetMethod("GetGameViewSize");

            if (getBuiltinCount == null || getCustomCount == null || getGameViewSize == null) return;

            int builtin = (int)getBuiltinCount.Invoke(group, null);
            int custom = (int)getCustomCount.Invoke(group, null);

            int total = builtin + custom;
            int foundIndex = -1;

            for (int i = 0; i < total; i++)
            {
                object gvSize = getGameViewSize.Invoke(group, new object[] { i });
                if (gvSize == null) continue;
                var widthProp = gvSize.GetType().GetProperty("width");
                var heightProp = gvSize.GetType().GetProperty("height");
                if (widthProp == null || heightProp == null) continue;

                int w = (int)widthProp.GetValue(gvSize, null);
                int h = (int)heightProp.GetValue(gvSize, null);

                if (w == width && h == height)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex < 0)
            {
                // Add custom size
                Type gvSizeType = Type.GetType("UnityEditor.GameViewSizeType,UnityEditor");
                if (gvSizeType == null) return;
                object fixedRes = Enum.Parse(gvSizeType, "FixedResolution");

                ConstructorInfo ctor = sizeType.GetConstructor(new[] { gvSizeType, typeof(int), typeof(int), typeof(string) });
                if (ctor == null) return;
                object newSize = ctor.Invoke(new object[] { fixedRes, width, height, width + "x" + height });

                MethodInfo addCustom = group.GetType().GetMethod("AddCustomSize");
                if (addCustom == null) return;
                addCustom.Invoke(group, new object[] { newSize });

                custom = (int)getCustomCount.Invoke(group, null);
                foundIndex = builtin + (custom - 1);
            }

            // Select size in GameView
            EditorWindow gv = GetMainGameView();
            if (gv == null) return;

            Type gvType = gv.GetType();
            PropertyInfo selectedSizeIndex = gvType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (selectedSizeIndex != null)
                selectedSizeIndex.SetValue(gv, foundIndex, null);

            gv.Repaint();
        }

        // Hidden MonoBehaviour to run coroutines from the editor tool
        private class CaptureHelper : MonoBehaviour { }
    }
}
