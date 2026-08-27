using UnityEditor;
using UnityEngine;


[ValidationRule]
public class MissingPrefabAssetRule : ValidationRule {

	public override string Id => "missing_prefab_asset";
	public override string Name => "Missing Prefab Asset";
	public override string Description => "Checks the scene for GameObjects with missing prefab assets.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;
	public override ValidationCategory Category => ValidationCategory.Prefabs;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(gameObject);

		if (prefabAssetType == PrefabAssetType.MissingAsset) {
			results.AddResult(Severity, Id, Name, "Missing prefab asset.", $"GameObject <i><b>{gameObject.name}</b></i> references a missing prefab asset.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}

}