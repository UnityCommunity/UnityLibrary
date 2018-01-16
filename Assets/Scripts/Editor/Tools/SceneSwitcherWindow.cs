
1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
29
30
31
32
33
34
35
36
37
38
39
40
41
42
43
44
45
46
47
48
49
50
51
52
53
54
55
56
57
58
59
60
61
62
63
64
65
66
67
68
69
70
71
72
73
74
75
76
77
78
79
80
81
82
83
84
85
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
 
public class SceneSwitcherWindow : EditorWindow
{
    
    public enum ScenesSource
    {
        Assets,
        BuildSettings
    }
 
    protected Vector2 scrollPosition;
    protected ScenesSource scenesSource = ScenesSource.Assets;
    protected OpenSceneMode openSceneMode = OpenSceneMode.Single;
 
    [MenuItem ( "Tools/Scene Switcher" )]
    public static void Init ()
    {
        var window = EditorWindow.GetWindow<SceneSwitcherWindow> ( "Scene Switcher" );
        window.Show ();
    }
 
    protected virtual void OnGUI ()
    {
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene> ( EditorBuildSettings.scenes );
        this.scenesSource = ( ScenesSource )EditorGUILayout.EnumPopup ( "Scenes Source", this.scenesSource );
        this.openSceneMode = ( OpenSceneMode )EditorGUILayout.EnumPopup ( "Open Scene Mode", this.openSceneMode );
        GUILayout.Label ( "Scenes", EditorStyles.boldLabel );
        string [] guids = AssetDatabase.FindAssets ( "t:Scene" );
        this.scrollPosition = EditorGUILayout.BeginScrollView ( this.scrollPosition );
        EditorGUILayout.BeginVertical ();
        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath ( guids [ i ] );
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset> ( path );
            EditorBuildSettingsScene buildScene = buildScenes.Find ( (editorBuildScene ) =>
            {
                return editorBuildScene.path == path;
            } );
            Scene scene = SceneManager.GetSceneByPath ( path );
            bool isOpen = scene.IsValid () && scene.isLoaded;
            GUI.enabled = !isOpen;
            if ( this.scenesSource == ScenesSource.Assets )
            {
                if ( GUILayout.Button ( sceneAsset.name ) )
                {
                    Open ( path );
                }
            }
            else
            {
                if ( buildScene != null )
                {
                    if ( GUILayout.Button ( sceneAsset.name ) )
                    {
                        Open ( path );
                    }
                }
            }
            GUI.enabled = true;
        }
        if ( GUILayout.Button ( "Create New Scene" ) )
        {
            Scene newScene = EditorSceneManager.NewScene ( NewSceneSetup.DefaultGameObjects, NewSceneMode.Single );
            EditorSceneManager.SaveScene ( newScene );
        }
        EditorGUILayout.EndVertical ();
        EditorGUILayout.EndScrollView ();
    }
 
    public virtual void Open ( string path )
    {
        if ( EditorSceneManager.EnsureUntitledSceneHasBeenSaved ( "You don't have saved the Untitled Scene, Do you want to leave?" ) )
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
            EditorSceneManager.OpenScene ( path, this.openSceneMode );
        }
    }
 
}
