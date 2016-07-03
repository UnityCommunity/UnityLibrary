using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// Hierarchy icons http://answers.unity3d.com/answers/1113260/view.html

// displays icons for tilemaps in hierarchy
// adds up/down arrows for tilemap layers (to arrange them up or down)

[InitializeOnLoad]
class TileMapHierarchyHelper
{
    static Texture2D tilemapIcon, tilemapIconDisabled, upArrow, downArrow;

    static TileMapHierarchyHelper()
    {
        tilemapIcon = AssetDatabase.LoadAssetAtPath("Assets/Icons/icon_tilemap.png", typeof(Texture2D)) as Texture2D;
        tilemapIconDisabled = AssetDatabase.LoadAssetAtPath("Assets/Icons/icon_tilemap_disabled.png", typeof(Texture2D)) as Texture2D;
        upArrow = AssetDatabase.LoadAssetAtPath("Assets/Icons/icon_UpArrow2.png", typeof(Texture2D)) as Texture2D;
        downArrow = AssetDatabase.LoadAssetAtPath("Assets/Icons/icon_DownArrow2.png", typeof(Texture2D)) as Texture2D;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCallBack;
    }


    static void HierarchyItemCallBack(int instanceID, Rect selectionRect)
    {
        // catch events inside hierarchy window
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
//            Vector3 mousePos = Event.current.mousePosition;
            //Debug.Log(selectionRect);
            //Debug.Log(mousePos);
        }

        Rect r = new Rect(selectionRect);
        var origX = r.x;
        r.x = r.width; //icon at end
        r.y = r.y + 4 + 1;
        r.width = 12; // for button

        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        //        if (go && Selection.activeGameObject == go && go.GetComponent<TileMap>())
        if (go && go.GetComponent<TileMap>())
        {
            // buttons
            if (GUI.Button(r, upArrow, GUIStyle.none))
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                MoveInHierarchy(go, -1);
            }
            r.x += 13;
            if (GUI.Button(r, downArrow, GUIStyle.none))
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                MoveInHierarchy(go, 1);
            }

            // icons
            r.x = origX - 12; // icon at front
            r.y -= 1;
            GUI.Label(r, go.activeInHierarchy ? tilemapIcon : tilemapIconDisabled);
        }
    }

    // http://answers.unity3d.com/answers/807156/view.html
    static void MoveInHierarchy(GameObject go, int delta)
    {
        go.transform.SetSiblingIndex(go.transform.GetSiblingIndex() + delta);
    }
}
