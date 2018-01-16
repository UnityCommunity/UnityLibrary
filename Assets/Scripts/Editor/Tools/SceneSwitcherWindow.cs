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
