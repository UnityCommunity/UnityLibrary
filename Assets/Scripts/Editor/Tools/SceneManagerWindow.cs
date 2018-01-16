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
