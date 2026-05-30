// editortool to capture thumbnails of meshes, prefabs, or gameobjects with custom settings and save as PNG

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Editor.Tools
{
    public class MeshThumbnailGrabberWindow : EditorWindow
    {
        private Object sourceObject;

        private PreviewRenderUtility previewUtility;

        private GameObject previewRoot;
        private GameObject previewInstance;

        private Texture2D exportPreviewTexture;

        private readonly List<PreviewDrawable> drawables = new List<PreviewDrawable>();

        private Vector2 orbit = new Vector2(135f, -20f);

        private float zoom = 1f;
        private float fitDistance = 3f;
        private float cameraDistanceMultiplier = 1f;
        private float lightIntensity = 1.25f;

        private int outputWidth = 512;
        private int outputHeight = 512;

        private Color backgroundColor = new Color(0f, 0f, 0f, 0f);
        private Color ambientColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        private Color lightColor = Color.white;

        private bool drawGround = false;

        private struct PreviewDrawable
        {
            public Mesh Mesh;
            public Matrix4x4 Matrix;
            public Material[] Materials;
        }

        [MenuItem("Tools/Thumbnail Grabber/Mesh Thumbnail Grabber")]
        public static void Open()
        {
            var win = GetWindow<MeshThumbnailGrabberWindow>("Mesh Thumbnail Grabber");
            win.minSize = new Vector2(500, 680);
        }

        private void OnEnable()
        {
            CreatePreviewUtility();
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void CreatePreviewUtility()
        {
            if (previewUtility != null)
            {
                previewUtility.Cleanup();
            }

            previewUtility = new PreviewRenderUtility(true);
            previewUtility.cameraFieldOfView = 30f;

            ApplyLightSettings();
        }

        private void Cleanup()
        {
            DestroyPreviewObjects();

            if (previewUtility != null)
            {
                previewUtility.Cleanup();
                previewUtility = null;
            }

            ClearExportPreview();
        }

        private void DestroyPreviewObjects()
        {
            drawables.Clear();

            if (previewRoot != null)
            {
                DestroyImmediate(previewRoot);
                previewRoot = null;
                previewInstance = null;
            }
        }

        private void ClearExportPreview()
        {
            if (exportPreviewTexture != null)
            {
                DestroyImmediate(exportPreviewTexture);
                exportPreviewTexture = null;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();

                sourceObject = EditorGUILayout.ObjectField(
                    "Source",
                    sourceObject,
                    typeof(Object),
                    false
                );

                if (EditorGUI.EndChangeCheck())
                {
                    ClearExportPreview();
                    CreatePreviewInstance();
                    FitToView();
                    Repaint();
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();

                outputWidth = Mathf.Clamp(EditorGUILayout.IntField("Output Width", outputWidth), 16, 8192);
                outputHeight = Mathf.Clamp(EditorGUILayout.IntField("Output Height", outputHeight), 16, 8192);

                if (EditorGUI.EndChangeCheck())
                {
                    ClearExportPreview();
                }

                zoom = EditorGUILayout.Slider("Zoom", zoom, 0.05f, 100f);
                cameraDistanceMultiplier = EditorGUILayout.Slider("Distance Multiplier", cameraDistanceMultiplier, 0.1f, 5f);

                EditorGUI.BeginChangeCheck();

                lightIntensity = EditorGUILayout.Slider("Light Intensity", lightIntensity, 0f, 5f);
                ambientColor = EditorGUILayout.ColorField("Ambient Color", ambientColor);
                lightColor = EditorGUILayout.ColorField("Light Color", lightColor);
                drawGround = EditorGUILayout.Toggle("Draw Ground", drawGround);

                if (EditorGUI.EndChangeCheck())
                {
                    ApplyLightSettings();
                    ClearExportPreview();

                    CenterObjectToOrigin();
                    CacheDrawables();
                    Repaint();
                }
            }

            Rect previewRect = GUILayoutUtility.GetRect(
                10,
                10000,
                10,
                10000,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );

            DrawInteractivePreview(previewRect);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = sourceObject != null;

                if (GUILayout.Button("Fit To View", GUILayout.Height(28)))
                {
                    FitToView();
                    ClearExportPreview();
                    Repaint();
                }

                if (GUILayout.Button("Reset Rotation", GUILayout.Height(28)))
                {
                    orbit = new Vector2(135f, -20f);
                    ClearExportPreview();
                    Repaint();
                }

                if (GUILayout.Button("Preview Export Size", GUILayout.Height(28)))
                {
                    UpdateExportPreview();
                }

                if (GUILayout.Button("Save PNG", GUILayout.Height(28)))
                {
                    SavePng();
                }

                GUI.enabled = true;
            }

            DrawExportPreviewPanel();
        }

        private void ApplyLightSettings()
        {
            if (previewUtility == null)
            {
                return;
            }

            previewUtility.lights[0].intensity = lightIntensity;
            previewUtility.lights[0].color = lightColor;
            previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);

            previewUtility.lights[1].intensity = lightIntensity * 0.35f;
            previewUtility.lights[1].color = lightColor;
            previewUtility.lights[1].transform.rotation = Quaternion.Euler(340f, 218f, 177f);
        }

        private void DrawInteractivePreview(Rect rect)
        {
            if (previewUtility == null)
            {
                CreatePreviewUtility();
            }

            if (sourceObject == null)
            {
                EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));
                GUI.Label(rect, "Assign a prefab, model, GameObject, or Mesh", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (previewInstance == null)
            {
                CreatePreviewInstance();
                FitToView();
            }

            HandlePreviewInput(rect);

            Texture texture = RenderPreview(rect.width, rect.height);

            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
            }
        }

        private void HandlePreviewInput(Rect rect)
        {
            Event e = Event.current;

            if (!rect.Contains(e.mousePosition))
            {
                return;
            }

            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                orbit += new Vector2(e.delta.x, -e.delta.y);
                orbit.y = Mathf.Clamp(orbit.y, -89f, 89f);

                ClearExportPreview();

                e.Use();
                Repaint();
            }

            if (e.type == EventType.ScrollWheel)
            {
                zoom *= 1f - e.delta.y * 0.08f;
                zoom = Mathf.Clamp(zoom, 0.05f, 100f);

                ClearExportPreview();

                e.Use();
                Repaint();
            }
        }

        private void CreatePreviewInstance()
        {
            DestroyPreviewObjects();

            if (sourceObject == null)
            {
                return;
            }

            previewRoot = new GameObject("Thumbnail Preview Root");
            previewRoot.hideFlags = HideFlags.HideAndDontSave;
            previewRoot.transform.position = Vector3.zero;
            previewRoot.transform.rotation = Quaternion.identity;
            previewRoot.transform.localScale = Vector3.one;

            GameObject sourceGameObject = sourceObject as GameObject;
            Mesh sourceMesh = sourceObject as Mesh;

            if (sourceGameObject != null)
            {
                previewInstance = Instantiate(sourceGameObject, previewRoot.transform);
                previewInstance.hideFlags = HideFlags.HideAndDontSave;
            }
            else if (sourceMesh != null)
            {
                previewInstance = new GameObject("Mesh Preview Instance");
                previewInstance.hideFlags = HideFlags.HideAndDontSave;
                previewInstance.transform.SetParent(previewRoot.transform, false);

                MeshFilter meshFilter = previewInstance.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = sourceMesh;

                MeshRenderer meshRenderer = previewInstance.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = GetDefaultMaterial();
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(sourceObject);
                GameObject loaded = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (loaded != null)
                {
                    previewInstance = Instantiate(loaded, previewRoot.transform);
                    previewInstance.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            if (previewInstance == null)
            {
                DestroyPreviewObjects();
                return;
            }

            DisableSceneOnlyComponents(previewInstance);

            previewInstance.transform.localPosition = Vector3.zero;
            previewInstance.transform.localRotation = Quaternion.identity;
            previewInstance.transform.localScale = Vector3.one;

            CenterObjectToOrigin();
            CacheDrawables();
        }

        private void CenterObjectToOrigin()
        {
            if (previewRoot == null || previewInstance == null)
            {
                return;
            }

            previewRoot.transform.position = Vector3.zero;
            previewRoot.transform.rotation = Quaternion.identity;
            previewRoot.transform.localScale = Vector3.one;

            previewInstance.transform.localPosition = Vector3.zero;

            Bounds bounds = GetWorldBounds(previewRoot);
            Vector3 centerOffset = bounds.center;

            previewInstance.transform.position -= centerOffset;

            if (drawGround)
            {
                Bounds centeredBounds = GetWorldBounds(previewRoot);
                previewInstance.transform.position -= new Vector3(0f, centeredBounds.min.y, 0f);
            }
        }

        private void FitToView()
        {
            if (previewInstance == null || previewUtility == null)
            {
                return;
            }

            CenterObjectToOrigin();
            CacheDrawables();

            Bounds bounds = GetWorldBounds(previewRoot);

            float radius = Mathf.Max(bounds.extents.magnitude, 0.001f);
            float fov = previewUtility.cameraFieldOfView;
            float fovRad = fov * Mathf.Deg2Rad;

            fitDistance = radius / Mathf.Sin(fovRad * 0.5f);
            fitDistance *= 1.15f;

            zoom = 1f;
        }

        private void CacheDrawables()
        {
            drawables.Clear();

            if (previewRoot == null)
            {
                return;
            }

            Renderer[] renderers = previewRoot.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();

                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    drawables.Add(new PreviewDrawable
                    {
                        Mesh = meshFilter.sharedMesh,
                        Matrix = renderer.localToWorldMatrix,
                        Materials = renderer.sharedMaterials
                    });

                    continue;
                }

                SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;

                if (skinned != null && skinned.sharedMesh != null)
                {
                    drawables.Add(new PreviewDrawable
                    {
                        Mesh = skinned.sharedMesh,
                        Matrix = skinned.localToWorldMatrix,
                        Materials = skinned.sharedMaterials
                    });
                }
            }
        }

        private Texture RenderPreview(float width, float height)
        {
            if (previewUtility == null || previewInstance == null)
            {
                return null;
            }

            Rect rect = new Rect(0f, 0f, Mathf.Max(1f, width), Mathf.Max(1f, height));

            previewUtility.BeginPreview(rect, GUIStyle.none);
            RenderCurrentView();
            return previewUtility.EndPreview();
        }

        private void RenderCurrentView()
        {
            Camera camera = previewUtility.camera;

            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = backgroundColor;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 10000f;
            camera.fieldOfView = previewUtility.cameraFieldOfView;
            camera.allowHDR = false;
            camera.allowMSAA = true;

            RenderSettings.ambientLight = ambientColor;

            Quaternion rotation = Quaternion.Euler(-orbit.y, orbit.x, 0f);

            float distance = fitDistance * cameraDistanceMultiplier / Mathf.Max(zoom, 0.001f);
            distance = Mathf.Max(distance, 0.01f);

            Vector3 cameraDirection = rotation * Vector3.forward;

            camera.transform.position = -cameraDirection * distance;
            camera.transform.rotation = Quaternion.LookRotation(cameraDirection, Vector3.up);

            ApplyLightSettings();

            DrawCachedRenderers();

            if (drawGround)
            {
                DrawGround();
            }

            camera.Render();
        }

        private void DrawCachedRenderers()
        {
            for (int i = 0; i < drawables.Count; i++)
            {
                PreviewDrawable drawable = drawables[i];

                if (drawable.Mesh == null)
                {
                    continue;
                }

                DrawMeshWithMaterials(drawable.Mesh, drawable.Matrix, drawable.Materials);
            }
        }

        private void DrawMeshWithMaterials(Mesh mesh, Matrix4x4 matrix, Material[] materials)
        {
            if (mesh == null)
            {
                return;
            }

            int subMeshCount = Mathf.Max(1, mesh.subMeshCount);

            for (int i = 0; i < subMeshCount; i++)
            {
                Material material = GetDefaultMaterial();

                if (materials != null && i < materials.Length && materials[i] != null)
                {
                    material = materials[i];
                }

                if (material == null)
                {
                    continue;
                }

                previewUtility.DrawMesh(mesh, matrix, material, i);
            }
        }

        private void DrawGround()
        {
            Mesh planeMesh = Resources.GetBuiltinResource<Mesh>("Plane.fbx");

            if (planeMesh == null)
            {
                return;
            }

            Bounds bounds = GetWorldBounds(previewRoot);

            float size = Mathf.Max(bounds.size.x, bounds.size.z, 1f) * 1.5f;

            Matrix4x4 matrix = Matrix4x4.TRS(
                new Vector3(0f, bounds.min.y, 0f),
                Quaternion.identity,
                new Vector3(size * 0.1f, 1f, size * 0.1f)
            );

            previewUtility.DrawMesh(planeMesh, matrix, GetDefaultMaterial(), 0);
        }

        private Bounds GetWorldBounds(GameObject root)
        {
            if (root == null)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            bool hasBounds = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            return bounds;
        }

        private static Material GetDefaultMaterial()
        {
            Material material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Standard");

            if (shader != null)
            {
                return new Material(shader);
            }

            return null;
        }

        private static void DisableSceneOnlyComponents(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }
            }

            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);

            foreach (Camera camera in cameras)
            {
                camera.enabled = false;
            }

            Light[] lights = root.GetComponentsInChildren<Light>(true);

            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }

        private void SavePng()
        {
            if (sourceObject == null || previewInstance == null)
            {
                return;
            }

            string defaultName = sourceObject.name + "_thumbnail_" + outputWidth + "x" + outputHeight + ".png";

            string path = EditorUtility.SaveFilePanel(
                "Save Thumbnail PNG",
                Application.dataPath,
                defaultName,
                "png"
            );

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Texture2D texture = RenderToTexture2D(outputWidth, outputHeight);

            if (texture == null)
            {
                Debug.LogError("Failed to render thumbnail.");
                return;
            }

            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(path, png);

            DestroyImmediate(texture);

            AssetDatabase.Refresh();

            PingSavedAsset(path);
            UpdateExportPreview();

            Debug.Log("Saved thumbnail: " + path);
        }

        private Texture2D RenderToTexture2D(int width, int height)
        {
            if (previewUtility == null || previewInstance == null)
            {
                return null;
            }

            Rect rect = new Rect(0f, 0f, width, height);

            previewUtility.BeginStaticPreview(rect);
            RenderCurrentView();

            Texture2D texture = previewUtility.EndStaticPreview();

            if (texture == null)
            {
                return null;
            }

            Texture2D copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            copy.SetPixels(texture.GetPixels());
            copy.Apply();

            return copy;
        }

        private static void PingSavedAsset(string absolutePath)
        {
            string projectPath = Path.GetFullPath(Application.dataPath + "/..").Replace("\\", "/");
            string fixedPath = Path.GetFullPath(absolutePath).Replace("\\", "/");

            if (!fixedPath.StartsWith(projectPath))
            {
                EditorUtility.RevealInFinder(absolutePath);
                return;
            }

            string assetPath = fixedPath.Substring(projectPath.Length + 1);

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                EditorUtility.RevealInFinder(absolutePath);
            }
        }

        private void UpdateExportPreview()
        {
            if (sourceObject == null || previewInstance == null)
            {
                return;
            }

            ClearExportPreview();

            exportPreviewTexture = RenderToTexture2D(outputWidth, outputHeight);
            Repaint();
        }

        private void DrawExportPreviewPanel()
        {
            if (exportPreviewTexture == null)
            {
                return;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    "Export Preview: " + exportPreviewTexture.width + "x" + exportPreviewTexture.height + " px",
                    EditorStyles.boldLabel
                );

                Rect rect = GUILayoutUtility.GetRect(
                    exportPreviewTexture.width,
                    exportPreviewTexture.height,
                    GUILayout.Width(exportPreviewTexture.width),
                    GUILayout.Height(exportPreviewTexture.height)
                );

                EditorGUI.DrawTextureTransparent(
                    rect,
                    exportPreviewTexture,
                    ScaleMode.StretchToFill
                );

                if (GUILayout.Button("Clear Preview"))
                {
                    ClearExportPreview();
                }
            }
        }

    } // class
} // namespace
