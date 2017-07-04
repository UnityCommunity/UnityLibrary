using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    GridGenerator t;

    private void OnEnable()
    {
        t = (GridGenerator) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Cleanup"))
        {
            CleanUp();
        }

        if (GUILayout.Button("Generate Grid"))
        {
            CleanUp();
            GenerateGrid();
        }
    }

    private void CleanUp()
    {
        List<Transform> tempList = t.transform.Cast<Transform>().ToList();
        tempList.ForEach(x => DestroyImmediate(x.gameObject));
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < t.SizeX; x++)
        {
            for (int y = 0; y < t.SizeY; y++)
            {
                Vector3 localOffset = new Vector3(
                    t.Offset.x * x,
                    0,
                    t.Offset.y * y
                );

                GameObject spawnedObject = Instantiate(t.PrefabToPlace);

                spawnedObject.transform.SetParent(t.transform);
                spawnedObject.transform.localPosition = localOffset;

                spawnedObject.name = string.Format("{0} ({1},{2})", t.PrefabToPlace.name, x, y);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

}
