
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
86
87
88
89
90
91
92
93
94
95
96
97
98
99
100
101
102
103
104
105
106
107
108
109
110
111
112
113
114
115
116
117
118
119
120
121
122
123
124
125
126
127
128
129
130
131
132
133
134
135
136
137
138
139
140
141
142
143
144
145
146
147
148
149
150
151
152
153
154
155
156
157
158
159
160
161
162
163
164
165
166
167
168
169
170
171
172
173
174
175
176
177
178
179
180
181
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
 
public class SceneManagerWindow : EditorWindow
{
    
    protected Vector2 scrollPosition;
    protected NewSceneSetup newSceneSetup = NewSceneSetup.DefaultGameObjects;
    protected NewSceneMode newSceneMode = NewSceneMode.Single;
    protected OpenSceneMode openSceneMode = OpenSceneMode.Single;
    protected bool showPath = false;
    protected bool showAddToBuild = true;
    protected bool askBeforeDelete = true;
    protected bool [] selectedScenes;
    protected string [] guids;
 
    [MenuItem ( "Tools/Scene Manager" )]
    public static void Init ()
    {
        var window = EditorWindow.GetWindow<SceneManagerWindow> ( "Scene Manager" );
        window.Show ();
    }
 
    protected virtual void OnGUI ()
    {
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene> ( EditorBuildSettings.scenes );
        GUILayout.Label ( "Scenes", EditorStyles.boldLabel );
        this.guids = AssetDatabase.FindAssets ( "t:Scene" );
        if ( this.selectedScenes == null || this.selectedScenes.Length != guids.Length )
        {
            this.selectedScenes = new bool[guids.Length];
        }
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
            EditorGUILayout.BeginHorizontal ();
            this.selectedScenes [ i ] = EditorGUILayout.Toggle ( this.selectedScenes [ i ], GUILayout.Width ( 15 ) );
            if ( isOpen )
            {
                GUILayout.Label ( sceneAsset.name, EditorStyles.whiteLabel );
            }
            else
            {
                GUILayout.Label ( sceneAsset.name, EditorStyles.wordWrappedLabel );
            }
            if ( this.showPath )
            {
                GUILayout.Label ( path, EditorStyles.wordWrappedLabel );
            }
            if ( buildScene == null )
            {
                if ( this.showAddToBuild )
                {
                    if ( GUILayout.Button ( "Add to Build" ) )
                    {
                        AddToBuild ( path );
                    }
                }
            }
            if ( GUILayout.Button ( isOpen ? "Close" : "Open", GUILayout.Width ( 50 ) ) )
            {
                if ( isOpen )
                {
                    EditorSceneManager.CloseScene ( scene, true );
                }
                else
                {
                    Open ( path );
                }
            }
            if ( GUILayout.Button ( "Delete", GUILayout.Width ( 50 ) ) )
            {
                Delete ( path );
            }
            EditorGUILayout.EndHorizontal ();
        }
        if ( GUILayout.Button ( "Create New Scene" ) )
        {
            Scene newScene = EditorSceneManager.NewScene ( this.newSceneSetup, this.newSceneMode );
            EditorSceneManager.SaveScene ( newScene );
        }
        EditorGUILayout.EndVertical ();
        EditorGUILayout.EndScrollView ();
        GUILayout.Label ( "Bulk Actions", EditorStyles.boldLabel );
        bool anySelected = false;
        for ( int i = 0; i < this.selectedScenes.Length; i++ )
        {
            anySelected |= this.selectedScenes [ i ];
        }
        GUI.enabled = anySelected;
        EditorGUILayout.BeginHorizontal ();
        if ( GUILayout.Button ( "Delete" ) )
        {
            for ( int i = 0; i < this.selectedScenes.Length; i++ )
            {
                if ( this.selectedScenes [ i ] )
                {
                    Delete ( AssetDatabase.GUIDToAssetPath ( this.guids [ i ] ) );
                }
            }
        }
        if ( GUILayout.Button ( "Open Additive" ) )
        {
            OpenSceneMode openMode = this.openSceneMode;
            this.openSceneMode = OpenSceneMode.Additive;
            for ( int i = 0; i < this.selectedScenes.Length; i++ )
            {
                if ( this.selectedScenes [ i ] )
                {
                    Open ( AssetDatabase.GUIDToAssetPath ( this.guids [ i ] ) );
                }
            }
            this.openSceneMode = openMode;
        }
        EditorGUILayout.EndHorizontal ();
        GUI.enabled = true;
        GUILayout.Label ( "Actions", EditorStyles.boldLabel );
        EditorGUILayout.BeginHorizontal ();
        if ( GUILayout.Button ( "Save Modified Scenes" ) )
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
        }
        if ( GUILayout.Button ( "Save Open Scenes" ) )
        {
            EditorSceneManager.SaveOpenScenes ();
        }
        EditorGUILayout.EndHorizontal ();
        GUILayout.Label ( "Settings", EditorStyles.boldLabel );
        this.newSceneSetup = ( NewSceneSetup )EditorGUILayout.EnumPopup ( "New Scene Setup", this.newSceneSetup );
        this.newSceneMode = ( NewSceneMode )EditorGUILayout.EnumPopup ( "New Scene Mode", this.newSceneMode );
        this.openSceneMode = ( OpenSceneMode )EditorGUILayout.EnumPopup ( "Open Scene Mode", this.openSceneMode );
        this.showPath = EditorGUILayout.Toggle ( "Show Path", this.showPath );
        this.showAddToBuild = EditorGUILayout.Toggle ( "Show Add To Build", this.showAddToBuild );
        this.askBeforeDelete = EditorGUILayout.Toggle ( "Ask Before Delete", this.askBeforeDelete );
    }
 
    public virtual void Open ( string path )
    {
        if ( EditorSceneManager.EnsureUntitledSceneHasBeenSaved ( "You don't have saved the Untitled Scene, Do you want to leave?" ) )
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
            EditorSceneManager.OpenScene ( path, this.openSceneMode );
        }
    }
 
    public virtual void Delete ( string path )
    {
        if ( !askBeforeDelete || EditorUtility.DisplayDialog (
                 "Delete Scene",
                 string.Format ( 
                     "Are you sure you want to delete the below scene: {0}",
                     path ),
                 "Delete",
                 "Cancel" ) )
        {
            AssetDatabase.DeleteAsset ( path );
            AssetDatabase.Refresh ();
        }
    }
 
    public virtual void AddToBuild ( string path )
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene> ( EditorBuildSettings.scenes );
        scenes.Add ( new EditorBuildSettingsScene ( path, true ) );
        EditorBuildSettings.scenes = scenes.ToArray ();
    }
    
}
