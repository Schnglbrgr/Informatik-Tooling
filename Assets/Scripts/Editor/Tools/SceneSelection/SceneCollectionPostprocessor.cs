using System.IO;
using UnityEditor;
using UnityEngine;


public class SceneCollectionPostprocessor : AssetPostprocessor {

	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string assetPath in importedAssets) {
			SceneCollection sceneCollection = AssetDatabase.LoadAssetAtPath<SceneCollection>(assetPath);

			if (!sceneCollection)
				continue;

			if (assetPath.StartsWith(SceneCollection.FolderPath + "/"))
				continue;

			string sourcePath = assetPath;

			EditorApplication.delayCall += () => { MoveToCorrectFolder(sourcePath); };
		}
	}


	private static void MoveToCorrectFolder(string sourcePath) {
		SceneCollection sceneCollection = AssetDatabase.LoadAssetAtPath<SceneCollection>(sourcePath);

		if (!sceneCollection)
			return;

		if (sourcePath.StartsWith(SceneCollection.FolderPath + "/"))
			return;

		if (!EnsureFolderExists(SceneCollection.FolderPath)) {
			Debug.LogError($"Cannot move SceneCollection because the target folder does not exist: {SceneCollection.FolderPath}");
			return;
		}

		string fileName = Path.GetFileName(sourcePath);

		if (string.IsNullOrEmpty(fileName))
			return;

		string targetPath = $"{SceneCollection.FolderPath}/{fileName}";

		targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);

		string error = AssetDatabase.MoveAsset(sourcePath, targetPath);

		if (!string.IsNullOrEmpty(error)) {
			Debug.LogError($"SceneCollection could not be moved from '{sourcePath}' to '{targetPath}': {error}");
			return;
		}

		Debug.Log($"SceneCollection moved to: {targetPath}");

		Object asset = AssetDatabase.LoadAssetAtPath<Object>(targetPath);

		if (asset) {
			Selection.activeObject = asset;
			EditorGUIUtility.PingObject(asset);
		}
	}


	private static bool EnsureFolderExists(string folderPath) {
		if (AssetDatabase.IsValidFolder(folderPath))
			return true;

		if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
			string newFolder = AssetDatabase.CreateFolder("Assets", "Resources");

			if (string.IsNullOrEmpty(newFolder)) {
				Debug.LogError("Could not create Assets/Resources.");
				return false;
			}
		}

		if (!AssetDatabase.IsValidFolder(folderPath)) {
			string sceneCollectionsGuid = AssetDatabase.CreateFolder("Assets/Resources", "SceneCollections");

			if (string.IsNullOrEmpty(sceneCollectionsGuid)) {
				Debug.LogError($"Could not create folder: {folderPath}");
				return false;
			}
		}

		return AssetDatabase.IsValidFolder(folderPath);
	}

}