/// <summary>
/// PivotAligner — Unity Editor Window
/// Align two 3D models by picking a custom pivot point (vertex or face center)
/// on the source model, then rotating/translating it around that point.
///
/// Place this file inside any  Editor/  folder in your project.
/// Open via:  Tools/UnityLibrary/Pivot Aligner
/// </summary>
using UnityEngine;
using UnityEditor;

namespace UnityLibrary.Tools
{
    public class PivotAligner : EditorWindow
    {
        enum PickMode { Vertex, Face }
        enum ToolState { Idle, Picking, Transforming }

        const string MENU_PATH = "Tools/Pivot Aligner";
        const float GIZMO_RADIUS = 0.06f;
        const float GIZMO_CROSS = 0.25f;

        [SerializeField] private GameObject sourceObject;
        [SerializeField] private PickMode pickMode = PickMode.Vertex;

        // Rotation
        private float rotX;
        private float rotY;
        private float rotZ;

        private bool showFinetune = false;
        private float fineTuneRange = 5f;
        private float fineX;
        private float fineY;
        private float fineZ;

        // Scale
        private bool showScale = true;
        private bool uniformScale = true;
        private float uniformScaleValue = 1f;
        private float scaleX = 1f;
        private float scaleY = 1f;
        private float scaleZ = 1f;

        // Position offset
        private bool showPosOffset = false;
        private float posOffsetX;
        private float posOffsetY;
        private float posOffsetZ;
        private float finePosRange = 0.1f;

        // Runtime state
        private ToolState state = ToolState.Idle;
        private Vector3 pivotWorld = Vector3.zero;
        private bool hasPivot = false;

        private Vector3 basePosition;
        private Quaternion baseRotation;
        private Vector3 baseScale;

        private Vector3 highlightPoint = Vector3.zero;
        private Vector3 highlightNormal = Vector3.up;
        private bool hasHighlight = false;

        private Vector2 scroll;

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle stateStyle;
        private GUIStyle subLabelStyle;
        private bool stylesInit;

        private int undoGroupIndex = 0;
        private int lastHotControl = 0;
        private string lastUndoLabel = "";

        [MenuItem(MENU_PATH)]
        public static void ShowWindow()
        {
            PivotAligner win = GetWindow<PivotAligner>("Pivot Aligner");
            win.minSize = new Vector2(440, 560);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            titleContent = new GUIContent("Pivot Aligner");
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            CancelPicking();
        }

        private void OnGUI()
        {
            InitStyles();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("PIVOT ALIGNER", headerStyle);
            EditorGUILayout.Space(2);
            DrawHR();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("1. Source Model", sectionStyle);

            EditorGUI.BeginChangeCheck();
            sourceObject = (GameObject)EditorGUILayout.ObjectField(
                "Game Object",
                sourceObject,
                typeof(GameObject),
                true);

            if (EditorGUI.EndChangeCheck())
                ResetTool();

            if (sourceObject == null)
            {
                EditorGUILayout.HelpBox("Assign a GameObject to begin.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("2. Pick Mode", sectionStyle);

            EditorGUILayout.BeginHorizontal();

            if (DrawModeButton("Vertex", pickMode == PickMode.Vertex))
            {
                pickMode = PickMode.Vertex;
                hasHighlight = false;
            }

            if (DrawModeButton("Face", pickMode == PickMode.Face))
            {
                pickMode = PickMode.Face;
                hasHighlight = false;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("3. Select Pivot Point", sectionStyle);

            using (new EditorGUI.DisabledScope(state == ToolState.Transforming))
            {
                if (state != ToolState.Picking)
                {
                    if (GUILayout.Button("Select Target Point in Scene", GUILayout.Height(30)))
                        BeginPicking();
                }
                else
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.55f, 0.15f);

                    if (GUILayout.Button("Cancel Picking", GUILayout.Height(30)))
                        CancelPicking();

                    GUI.backgroundColor = prev;

                    string modeText = pickMode == PickMode.Vertex ? "vertex" : "face center";
                    EditorGUILayout.HelpBox(
                        "Hover over the model. The nearest " + modeText + " will highlight. Click to confirm pivot.",
                        MessageType.None);
                }
            }

            if (hasPivot)
            {
                EditorGUILayout.Space(2);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Pivot " + pivotWorld.ToString("F4"), subLabelStyle);

                    if (GUILayout.Button("Re-pick", GUILayout.Width(56), GUILayout.Height(18)))
                        BeginPicking();
                }
            }

            EditorGUILayout.Space(8);
            DrawHR();
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("4. Rotation Around Pivot", sectionStyle);

            using (new EditorGUI.DisabledScope(!hasPivot))
            {
                bool dirty = false;

                EditorGUI.BeginChangeCheck();

                DrawRotRow("Pitch X", ref rotX);
                DrawRotRow("Yaw Y", ref rotY);
                DrawRotRow("Roll Z", ref rotZ);

                if (EditorGUI.EndChangeCheck())
                    dirty = true;

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    showFinetune = EditorGUILayout.ToggleLeft("Fine-tune +/-", showFinetune, GUILayout.Width(102));

                    using (new EditorGUI.DisabledScope(!showFinetune))
                        fineTuneRange = Mathf.Max(0.001f, EditorGUILayout.FloatField(fineTuneRange, GUILayout.Width(52)));

                    EditorGUILayout.LabelField("deg", subLabelStyle, GUILayout.Width(28));
                }

                if (showFinetune)
                {
                    EditorGUI.BeginChangeCheck();

                    fineX = DrawFineRotSlider("Delta X", fineX, fineTuneRange);
                    fineY = DrawFineRotSlider("Delta Y", fineY, fineTuneRange);
                    fineZ = DrawFineRotSlider("Delta Z", fineZ, fineTuneRange);

                    if (EditorGUI.EndChangeCheck())
                        dirty = true;
                }

                if (dirty && hasPivot)
                    ApplyAll("Pivot Aligner - Rotate");

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset All Rotation", GUILayout.Height(24)))
                        ResetRotation();

                    if (showFinetune && GUILayout.Button("Reset Fine", GUILayout.Width(80), GUILayout.Height(24)))
                    {
                        fineX = 0f;
                        fineY = 0f;
                        fineZ = 0f;

                        if (hasPivot)
                            ApplyAll("Pivot Aligner - Rotate");
                    }
                }
            }

            EditorGUILayout.Space(8);
            DrawHR();
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                showScale = EditorGUILayout.Foldout(showScale, "5. Scale Around Pivot", true, sectionStyle);
                EditorGUILayout.LabelField("(uses selected point)", subLabelStyle);
            }

            if (showScale)
            {
                using (new EditorGUI.DisabledScope(!hasPivot))
                {
                    EditorGUI.BeginChangeCheck();

                    uniformScale = EditorGUILayout.ToggleLeft("Uniform Scale", uniformScale);

                    if (uniformScale)
                    {
                        uniformScaleValue = EditorGUILayout.FloatField("Scale", uniformScaleValue);
                        uniformScaleValue = Mathf.Max(0.0001f, uniformScaleValue);

                        scaleX = uniformScaleValue;
                        scaleY = uniformScaleValue;
                        scaleZ = uniformScaleValue;
                    }
                    else
                    {
                        scaleX = Mathf.Max(0.0001f, EditorGUILayout.FloatField("Scale X", scaleX));
                        scaleY = Mathf.Max(0.0001f, EditorGUILayout.FloatField("Scale Y", scaleY));
                        scaleZ = Mathf.Max(0.0001f, EditorGUILayout.FloatField("Scale Z", scaleZ));

                        uniformScaleValue = scaleX;
                    }

                    if (EditorGUI.EndChangeCheck() && hasPivot)
                        ApplyAll("Pivot Aligner - Scale");

                    EditorGUILayout.Space(3);

                    if (GUILayout.Button("Reset Scale", GUILayout.Height(24)))
                        ResetScale();
                }
            }

            EditorGUILayout.Space(8);
            DrawHR();
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                showPosOffset = EditorGUILayout.Foldout(showPosOffset, "6. Position Offset", true, sectionStyle);
                EditorGUILayout.LabelField("(moves pivot too)", subLabelStyle);
            }

            if (showPosOffset)
            {
                using (new EditorGUI.DisabledScope(!hasPivot))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Slider range +/-", subLabelStyle, GUILayout.Width(102));
                        finePosRange = Mathf.Max(0.0001f, EditorGUILayout.FloatField(finePosRange, GUILayout.Width(52)));
                        EditorGUILayout.LabelField("m", subLabelStyle, GUILayout.Width(14));
                    }

                    EditorGUILayout.Space(2);

                    EditorGUI.BeginChangeCheck();

                    posOffsetX = DrawOffsetSliderRow("Offset X", posOffsetX, finePosRange);
                    posOffsetY = DrawOffsetSliderRow("Offset Y", posOffsetY, finePosRange);
                    posOffsetZ = DrawOffsetSliderRow("Offset Z", posOffsetZ, finePosRange);

                    if (EditorGUI.EndChangeCheck() && hasPivot)
                        ApplyAll("Pivot Aligner - Move");

                    EditorGUILayout.Space(3);

                    if (GUILayout.Button("Reset Position Offset", GUILayout.Height(24)))
                        ResetPositionOffset();
                }
            }

            EditorGUILayout.Space(8);
            DrawHR();
            EditorGUILayout.Space(4);

            using (new EditorGUI.DisabledScope(!hasPivot))
            {
                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.22f, 0.80f, 0.40f);

                if (GUILayout.Button("Apply and Clear Pivot", GUILayout.Height(36)))
                    Apply();

                GUI.backgroundColor = prev;
            }

            if (hasPivot)
            {
                if (GUILayout.Button("Cancel and Revert to Original", GUILayout.Height(26)))
                    RevertAndReset();
            }

            EditorGUILayout.Space(4);

            string totalInfo = "";

            if (hasPivot)
            {
                totalInfo =
                    "rot(" +
                    (rotX + fineX).ToString("F3") + ", " +
                    (rotY + fineY).ToString("F3") + ", " +
                    (rotZ + fineZ).ToString("F3") + ") deg  scale(" +
                    scaleX.ToString("F3") + ", " +
                    scaleY.ToString("F3") + ", " +
                    scaleZ.ToString("F3") + ")  pos offset(" +
                    posOffsetX.ToString("F4") + ", " +
                    posOffsetY.ToString("F4") + ", " +
                    posOffsetZ.ToString("F4") + ") m";
            }

            string stateLabel;

            switch (state)
            {
                case ToolState.Picking:
                    stateLabel = "PICKING";
                    break;

                case ToolState.Transforming:
                    stateLabel = "TRANSFORMING   " + totalInfo;
                    break;

                default:
                    stateLabel = "idle";
                    break;
            }

            EditorGUILayout.LabelField(stateLabel, stateStyle);
            EditorGUILayout.Space(4);

            EditorGUILayout.EndScrollView();

            SceneView.RepaintAll();
        }

        private bool DrawModeButton(string label, bool active)
        {
            Color prev = GUI.backgroundColor;

            if (active)
                GUI.backgroundColor = new Color(0.3f, 0.65f, 1f);

            bool clicked = GUILayout.Button(label, GUILayout.Height(26));

            GUI.backgroundColor = prev;

            return clicked;
        }

        private void DrawRotRow(string label, ref float value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(72));

                value = EditorGUILayout.FloatField(value, GUILayout.Width(74));

                if (GUILayout.Button("-90", GUILayout.Width(36), GUILayout.Height(18)))
                    value -= 90f;

                if (GUILayout.Button("-45", GUILayout.Width(36), GUILayout.Height(18)))
                    value -= 45f;

                if (GUILayout.Button("0", GUILayout.Width(28), GUILayout.Height(18)))
                    value = 0f;

                if (GUILayout.Button("+45", GUILayout.Width(36), GUILayout.Height(18)))
                    value += 45f;

                if (GUILayout.Button("+90", GUILayout.Width(36), GUILayout.Height(18)))
                    value += 90f;
            }
        }

        private float DrawFineRotSlider(string label, float value, float range)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, subLabelStyle, GUILayout.Width(76));

                value = GUILayout.HorizontalSlider(value, -range, range);
                value = EditorGUILayout.FloatField(value, GUILayout.Width(64));

                EditorGUILayout.LabelField("deg", subLabelStyle, GUILayout.Width(28));
            }

            return value;
        }

        private float DrawOffsetSliderRow(string label, float value, float range)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(72));

                value = GUILayout.HorizontalSlider(value, -range, range);
                value = EditorGUILayout.FloatField(value, GUILayout.Width(74));

                EditorGUILayout.LabelField("m", subLabelStyle, GUILayout.Width(14));

                if (GUILayout.Button("0", GUILayout.Width(24), GUILayout.Height(18)))
                    value = 0f;
            }

            return value;
        }

        private void OnSceneGUI(SceneView sv)
        {
            if (sourceObject == null)
                return;

            if (hasPivot)
                DrawPivotGizmo(pivotWorld, Color.cyan);

            if (state == ToolState.Picking)
                HandlePicking(sv);
        }

        private void HandlePicking(SceneView sv)
        {
            Event e = Event.current;

            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (e.type == EventType.MouseMove || e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                TryRaycast(ray, out hasHighlight, out highlightPoint, out highlightNormal);

                if (hasHighlight)
                    DrawPivotGizmo(highlightPoint, new Color(1f, 0.8f, 0.1f, 0.9f));

                if (e.type == EventType.MouseDown && e.button == 0 && hasHighlight)
                {
                    ConfirmPivot(highlightPoint);
                    e.Use();
                }

                sv.Repaint();
            }

            if (hasHighlight)
                DrawPivotGizmo(highlightPoint, new Color(1f, 0.8f, 0.1f, 0.9f));
        }

        private void TryRaycast(Ray ray, out bool hit, out Vector3 point, out Vector3 normal)
        {
            hit = false;
            point = Vector3.zero;
            normal = Vector3.up;

            MeshFilter[] filters = sourceObject.GetComponentsInChildren<MeshFilter>();
            float bestDist = float.MaxValue;

            foreach (MeshFilter mf in filters)
            {
                if (mf.sharedMesh == null)
                    continue;

                Mesh mesh = mf.sharedMesh;
                Transform t = mf.transform;

                Ray localRay = new Ray(
                    t.InverseTransformPoint(ray.origin),
                    t.InverseTransformDirection(ray.direction).normalized);

                Vector3[] verts = mesh.vertices;
                int[] tris = mesh.triangles;
                Vector3[] normals = mesh.normals;

                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector3 v0 = verts[tris[i]];
                    Vector3 v1 = verts[tris[i + 1]];
                    Vector3 v2 = verts[tris[i + 2]];

                    float dist;
                    float u;
                    float v;

                    if (!RayTriangle(localRay, v0, v1, v2, out dist, out u, out v))
                        continue;

                    if (dist < 0f || dist >= bestDist)
                        continue;

                    bestDist = dist;
                    hit = true;

                    if (pickMode == PickMode.Vertex)
                    {
                        float w = 1f - u - v;
                        int vi = FindNearestVertex(u, v, w);

                        if (vi == 0)
                            point = t.TransformPoint(v0);
                        else if (vi == 1)
                            point = t.TransformPoint(v1);
                        else
                            point = t.TransformPoint(v2);
                    }
                    else
                    {
                        point = t.TransformPoint((v0 + v1 + v2) / 3f);
                    }

                    Vector3 n0 = normals.Length > tris[i] ? normals[tris[i]] : Vector3.up;
                    Vector3 n1 = normals.Length > tris[i + 1] ? normals[tris[i + 1]] : Vector3.up;
                    Vector3 n2 = normals.Length > tris[i + 2] ? normals[tris[i + 2]] : Vector3.up;

                    normal = t.TransformDirection((n0 * (1f - u - v) + n1 * u + n2 * v).normalized);
                }
            }
        }

        private static bool RayTriangle(
            Ray ray,
            Vector3 v0,
            Vector3 v1,
            Vector3 v2,
            out float dist,
            out float u,
            out float v)
        {
            dist = 0f;
            u = 0f;
            v = 0f;

            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            Vector3 h = Vector3.Cross(ray.direction, e2);

            float det = Vector3.Dot(e1, h);

            if (Mathf.Abs(det) < 1e-6f)
                return false;

            float f = 1f / det;
            Vector3 s = ray.origin - v0;

            u = f * Vector3.Dot(s, h);

            if (u < 0f || u > 1f)
                return false;

            Vector3 q = Vector3.Cross(s, e1);

            v = f * Vector3.Dot(ray.direction, q);

            if (v < 0f || u + v > 1f)
                return false;

            dist = f * Vector3.Dot(e2, q);

            return dist > 1e-5f;
        }

        private static int FindNearestVertex(float u, float v, float w)
        {
            if (w >= u && w >= v)
                return 0;

            if (u >= w && u >= v)
                return 1;

            return 2;
        }

        private void DrawPivotGizmo(Vector3 pos, Color color)
        {
            Handles.color = color;

            float handleSize = HandleUtility.GetHandleSize(pos);

            Handles.SphereHandleCap(
                0,
                pos,
                Quaternion.identity,
                GIZMO_RADIUS * handleSize,
                EventType.Repaint);

            float sz = GIZMO_CROSS * handleSize;

            Handles.DrawLine(pos - Vector3.right * sz, pos + Vector3.right * sz);
            Handles.DrawLine(pos - Vector3.up * sz, pos + Vector3.up * sz);
            Handles.DrawLine(pos - Vector3.forward * sz, pos + Vector3.forward * sz);

            GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
            s.normal.textColor = color;

            string label = hasPivot && Vector3.Distance(pos, pivotWorld) < 0.0001f ? "PIVOT" : "point";

            Handles.Label(pos + Vector3.up * sz * 1.5f, label, s);
        }

        private void BeginPicking()
        {
            if (sourceObject == null)
                return;

            state = ToolState.Picking;
            hasHighlight = false;

            SceneView.RepaintAll();
        }

        private void CancelPicking()
        {
            if (state == ToolState.Picking)
                state = hasPivot ? ToolState.Transforming : ToolState.Idle;

            hasHighlight = false;

            SceneView.RepaintAll();
        }

        private void ConfirmPivot(Vector3 worldPoint)
        {
            if (hasPivot && state == ToolState.Transforming)
                RevertTransform();

            pivotWorld = worldPoint;
            hasPivot = true;
            state = ToolState.Transforming;
            hasHighlight = false;

            basePosition = sourceObject.transform.position;
            baseRotation = sourceObject.transform.rotation;
            baseScale = sourceObject.transform.localScale;

            rotX = 0f;
            rotY = 0f;
            rotZ = 0f;

            fineX = 0f;
            fineY = 0f;
            fineZ = 0f;

            scaleX = 1f;
            scaleY = 1f;
            scaleZ = 1f;
            uniformScaleValue = 1f;

            posOffsetX = 0f;
            posOffsetY = 0f;
            posOffsetZ = 0f;

            Repaint();
            SceneView.RepaintAll();
        }

        private void ApplyAll(string undoLabel = "Pivot Aligner")
        {
            if (sourceObject == null || !hasPivot)
                return;

            int hot = GUIUtility.hotControl;

            if (hot != lastHotControl || undoLabel != lastUndoLabel)
            {
                undoGroupIndex++;
                lastHotControl = hot;
                lastUndoLabel = undoLabel;
            }

            Undo.RecordObject(sourceObject.transform, undoLabel);

            Quaternion deltaRotation = Quaternion.Euler(
                rotX + fineX,
                rotY + fineY,
                rotZ + fineZ);

            Vector3 scaleMultiplier = new Vector3(scaleX, scaleY, scaleZ);
            Vector3 positionOffset = new Vector3(posOffsetX, posOffsetY, posOffsetZ);

            Vector3 baseOffset = basePosition - pivotWorld;

            Vector3 scaledOffset = new Vector3(
                baseOffset.x * scaleMultiplier.x,
                baseOffset.y * scaleMultiplier.y,
                baseOffset.z * scaleMultiplier.z);

            sourceObject.transform.position =
                pivotWorld +
                deltaRotation * scaledOffset +
                positionOffset;

            sourceObject.transform.rotation = deltaRotation * baseRotation;
            sourceObject.transform.localScale = Vector3.Scale(baseScale, scaleMultiplier);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup() - undoGroupIndex + 1);
        }

        private void ResetRotation()
        {
            rotX = 0f;
            rotY = 0f;
            rotZ = 0f;

            fineX = 0f;
            fineY = 0f;
            fineZ = 0f;

            ApplyAll("Pivot Aligner - Reset Rotation");
        }

        private void ResetScale()
        {
            scaleX = 1f;
            scaleY = 1f;
            scaleZ = 1f;
            uniformScaleValue = 1f;

            ApplyAll("Pivot Aligner - Reset Scale");
        }

        private void ResetPositionOffset()
        {
            posOffsetX = 0f;
            posOffsetY = 0f;
            posOffsetZ = 0f;

            ApplyAll("Pivot Aligner - Reset Offset");
        }

        private void Apply()
        {
            if (sourceObject == null)
                return;

            Undo.SetCurrentGroupName("Pivot Aligner Apply");
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            ResetTool();
        }

        private void RevertAndReset()
        {
            RevertTransform();
            ResetTool();
        }

        private void RevertTransform()
        {
            if (sourceObject == null || !hasPivot)
                return;

            Undo.RecordObject(sourceObject.transform, "Pivot Aligner Revert");

            sourceObject.transform.position = basePosition;
            sourceObject.transform.rotation = baseRotation;
            sourceObject.transform.localScale = baseScale;
        }

        private void ResetTool()
        {
            state = ToolState.Idle;

            hasPivot = false;
            hasHighlight = false;

            rotX = 0f;
            rotY = 0f;
            rotZ = 0f;

            fineX = 0f;
            fineY = 0f;
            fineZ = 0f;

            scaleX = 1f;
            scaleY = 1f;
            scaleZ = 1f;
            uniformScaleValue = 1f;

            posOffsetX = 0f;
            posOffsetY = 0f;
            posOffsetZ = 0f;

            SceneView.RepaintAll();
            Repaint();
        }

        private void InitStyles()
        {
            if (stylesInit)
                return;

            stylesInit = true;

            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(0.65f, 0.88f, 1f);

            sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            sectionStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            stateStyle = new GUIStyle(EditorStyles.miniLabel);
            stateStyle.alignment = TextAnchor.MiddleRight;
            stateStyle.normal.textColor = new Color(0.40f, 0.75f, 0.50f);

            subLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            subLabelStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
        }

        private void DrawHR()
        {
            Rect r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(0.35f, 0.35f, 0.35f, 0.6f));
        }
    }
}
