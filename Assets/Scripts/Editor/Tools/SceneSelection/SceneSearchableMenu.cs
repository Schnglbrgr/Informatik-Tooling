using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;


public class SceneSearchableMenu : ScriptableObject, ISearchWindowProvider {

	private const string SceneCollectionsFolder = "SceneCollections";


	public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
		List<SearchTreeEntry> tree = new List<SearchTreeEntry>();

		SearchTreeGroupEntry group = new SearchTreeGroupEntry(new GUIContent("Scene Asset"));
		tree.Add(group);

		SearchTreeGroupEntry collections = new SearchTreeGroupEntry(new GUIContent("Collections"), 1);
		tree.Add(collections);

		SceneCollection[] sceneCollections = Resources.LoadAll<SceneCollection>(SceneCollectionsFolder);
		foreach (SceneCollection sceneCollection in sceneCollections) {
			SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(sceneCollection.name));
			entry.level = 2;
			entry.userData = sceneCollection;
			tree.Add(entry);
		}

		SearchTreeGroupEntry scenes = new SearchTreeGroupEntry(new GUIContent("Scenes", EditorGUIUtility.IconContent("SceneAsset Icon").image), 1);
		tree.Add(scenes);

		string[] guids = AssetDatabase.FindAssets("t:SceneAsset");
		foreach (string guid in guids) {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
			SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(asset.name, EditorGUIUtility.ObjectContent(asset, typeof(SceneCollection)).image));
			entry.level = 2;
			entry.userData = asset;
			tree.Add(entry);
		}

		return tree;
	}


	public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
		if (searchTreeEntry.userData is SceneCollection collection) {
			collection.Open();
			return true;
		}

		if (searchTreeEntry.userData is SceneAsset asset) {
			TryOpenScene(AssetDatabase.GetAssetPath(asset));
			return true;
		}

		return false;
	}


	private bool TryOpenScene(string scenePath) {
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return false;

		EditorSceneManager.OpenScene(scenePath);
		return true;
	}

}