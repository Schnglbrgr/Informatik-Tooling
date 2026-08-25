using UnityEngine;


[ValidationRule]
public sealed class MissingScriptRule : ValidationRule {

	public override string Id => "missing_script";
	public override string Name => "Missing Script";

	public override string Description => "Checks the scene for GameObjects containing missing scripts.";

	public override ValidationCategory Category => ValidationCategory.References;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		Component[] components = gameObject.GetComponents<Component>();

		foreach (Component component in components) {
			if (!component)
				results.Error(Id, Name, "Missing script detected.",
					$"GameObject '{gameObject.name}' contains a missing script. " + "Missing scripts can cause unexpected behaviour " +
					"and should normally be removed or restored.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}

	}

}