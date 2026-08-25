using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


[CreateAssetMenu(fileName = "NewSceneCollection", menuName = "Scene Collection/Scene Collection")]
public class SceneCollection : ScriptableObject {

	public const string FolderPath = "Assets/Resources/SceneCollections";

	[SerializeField] private SceneAsset[] sceneAssets;


	public void Open() {
		if (sceneAssets.Length == 0 || sceneAssets == null) {
			Debug.LogWarning($"There are no Scene Assets in {name}");
			return;
		}

		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return;

		int index = 0;

		foreach (SceneAsset asset in sceneAssets) {
			if (!asset) {
				Debug.LogWarning($"There is no Scene Asset in {name} at element: {index}");
				index++;
				continue;
			}

			string path = AssetDatabase.GetAssetPath(asset);

			EditorSceneManager.OpenScene(path, (index == 0) ? OpenSceneMode.Single : OpenSceneMode.Additive);
			index++;
		}

	}

}