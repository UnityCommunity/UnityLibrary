using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TerrainTreeReplacer : EditorWindow
{
    private const string RootObjectName = "TREES_CONVERTED";

    [MenuItem("Window/Tools/Terrain Tree Replacer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TerrainTreeReplacer));
    }

    private Terrain terrain;

    private bool disableDrawTreesAndFoliage = false;
    private int treeDivisions = 0;

    private bool DivideTreesIntoGroups { get { return treeDivisions > 0; } }

    void OnGUI()
    {
        GUILayout.Label("Replace Terrain Trees with Objects", EditorStyles.boldLabel);

        terrain = EditorGUILayout.ObjectField("Terrain:", terrain, typeof(Terrain), true) as Terrain;
        disableDrawTreesAndFoliage = EditorGUILayout.ToggleLeft("Disable Drawing Trees and Foliage", disableDrawTreesAndFoliage);

        GUILayout.Label("Tree Division groups: " + treeDivisions);
        treeDivisions = (int)GUILayout.HorizontalSlider(treeDivisions, 0, 10);

        if (GUILayout.Button("Replace Terrain trees to Objects!")) Replace();
        if (GUILayout.Button("Clear generated trees!")) Clear();
    }

    public void Replace()
    {
        if (terrain == null)
        {
            Debug.LogError("Please Assign Terrain");
            return;
        }

        Clear();

        GameObject treeParent = new GameObject(RootObjectName);

        List<List<Transform>> treegroups = new List<List<Transform>>();

        if (DivideTreesIntoGroups)
        {
            for (int i = 0; i < treeDivisions; i++)
            {
                treegroups.Add(new List<Transform>());
                for (int j = 0; j < treeDivisions; j++)
                {
                    GameObject treeGroup = new GameObject("TreeGroup_" + i + "_" + j);
                    treeGroup.transform.parent = treeParent.transform;
                    treegroups[i].Add(treeGroup.transform);
                }
            }
        }

        TerrainData terrainData = terrain.terrainData;

        float xDiv = terrainData.size.x / (float)treeDivisions;
        float zDiv = terrainData.size.z / (float)treeDivisions;

        foreach (TreeInstance tree in terrainData.treeInstances)
        {
            GameObject treePrefab = terrainData.treePrototypes[tree.prototypeIndex].prefab;

            Vector3 position = Vector3.Scale(tree.position, terrainData.size);
            int xGroup = (int)(position.x / xDiv);
            int zGroup = (int)(position.z / zDiv);

            position += terrain.transform.position;

            Vector2 lookRotationVector = new Vector2(Mathf.Cos(tree.rotation - Mathf.PI), Mathf.Sin(tree.rotation - Mathf.PI));
            Quaternion rotation = Quaternion.LookRotation(new Vector3(lookRotationVector.x, 0, lookRotationVector.y), Vector3.up);

            Vector3 scale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);

            GameObject spawnedTree = Instantiate(treePrefab, position, rotation) as GameObject;
            spawnedTree.name = treePrefab.name;

            spawnedTree.transform.localScale = scale;

            if (DivideTreesIntoGroups) spawnedTree.transform.SetParent(treegroups[xGroup][zGroup]);
            else spawnedTree.transform.SetParent(treeParent.transform);
        }

        if (disableDrawTreesAndFoliage) terrain.drawTreesAndFoliage = false;
    }

    public void Clear()
    {
        DestroyImmediate(GameObject.Find(RootObjectName));
    }
}