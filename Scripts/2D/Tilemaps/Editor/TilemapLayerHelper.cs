using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// TilemapLayerHelper (wip)
// Use PageDown/PageUp to select between tilemap layers
// First assign tileRoot into the editorwindow field and hit GetTileMaps button


public class TilemapLayerHelper : EditorWindow
{
    Transform tileRoot;
    GameObject[] tilemapGos;

    int selectedLayer = 0;
    string[] layerNames = new string[] { "" };

    [MenuItem("Window/FloatingLayerManager")]
    static void Init()
    {
        TilemapLayerHelper window = (TilemapLayerHelper)EditorWindow.GetWindow(typeof(TilemapLayerHelper));
        window.titleContent = new GUIContent("TilemapLayerHelper");
        window.minSize = new Vector2(320, 128);
        window.Show();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("tileRoot", EditorStyles.boldLabel);
        tileRoot = (Transform)EditorGUILayout.ObjectField("", tileRoot, typeof(Transform), true);

        // get list of tilemap layers
        if (GUILayout.Button("GetLayers"))
        {
            var childTileMaps = tileRoot.GetComponentsInChildren<TileMap>();
            layerNames = new string[childTileMaps.Length];
            tilemapGos = new GameObject[childTileMaps.Length];

            int i = 0;
            foreach (var tm in childTileMaps)
            {
                layerNames[i] = tm.name;
                tilemapGos[i] = tm.gameObject;
                i++;
            }
        }
    }


    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.PageUp:
                    selectedLayer = Wrap(--selectedLayer, layerNames.Length);
                    Selection.activeGameObject = tilemapGos[selectedLayer];
                    break;

                case KeyCode.PageDown:
                    selectedLayer = Wrap(++selectedLayer, layerNames.Length);
                    Selection.activeGameObject = tilemapGos[selectedLayer];
                    break;

                default:
                    break;
            }
            e.Use();
        }


        Handles.BeginGUI();
        selectedLayer = GUI.SelectionGrid(new Rect(0, 32, 128, 64), selectedLayer, layerNames, 1);
        Handles.EndGUI();
    }


    // helpers
    int Wrap(int i, int i_max)
    {
        return ((i % i_max) + i_max) % i_max;
    }

}
